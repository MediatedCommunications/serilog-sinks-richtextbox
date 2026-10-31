#region Copyright 2021 C. Augusto Proiete & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Threading;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Abstraction;
using Serilog.Sinks.RichTextBox.Output;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox
{
    internal sealed class RichTextBoxSink : ILogEventSink, IDisposable
    {
        private const int MaxLinesPerTurn = 40;
        private static readonly TimeSpan CoalesceWindow = TimeSpan.FromMilliseconds(16);

        private readonly IRichTextBox _richTextBox;
        private readonly RichTextOutputTemplateRenderer _formatter;

        private readonly ChannelReader<LogEvent> _messageReader;
        private readonly ChannelWriter<LogEvent> _messageWriter;
        private readonly CancellationTokenSource _shutdown = new();
        private readonly Task _consumer;
        private int _disposeOnce;

        public RichTextBoxSink(IRichTextBox richTextBox, RichTextOutputTemplateRenderer formatter, int? retainedLineLimit = null)
        {
            _richTextBox = richTextBox ?? throw new ArgumentNullException(nameof(richTextBox));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            var chan = retainedLineLimit is int limit && limit > 0
                ? Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(limit)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    AllowSynchronousContinuations = false,
                    SingleReader = true,
                    SingleWriter = false,
                })
                : Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions
                {
                    AllowSynchronousContinuations = false,
                    SingleReader = true,
                    SingleWriter = false,
                });

            _messageReader = chan.Reader;
            _messageWriter = chan.Writer;

            _consumer = Task.Run(ProcessMessages);
        }

        private async Task ProcessMessages()
        {
            List<RichTextLine>? pending = null;
            var announcedFailure = false;
            try
            {
                while (true)
                {
                    if (pending == null)
                    {
                        if (!await _messageReader.WaitToReadAsync().ConfigureAwait(false))
                        {
                            break;
                        }

                        var batch = new List<RichTextLine>(MaxLinesPerTurn);
                        FillBatch(batch);
                        if (batch.Count < MaxLinesPerTurn && !_messageReader.Completion.IsCompleted)
                        {
                            await CoalesceAsync(batch).ConfigureAwait(false);
                        }

                        if (batch.Count == 0)
                        {
                            continue;
                        }

                        pending = batch;
                    }

                    var batchToWrite = pending;
                    try
                    {
                        await _richTextBox.WriteAsync(batchToWrite).ConfigureAwait(false);
                        pending = null;
                        announcedFailure = false;
                    }
                    catch (Exception ex)
                    {
                        if (!announcedFailure)
                        {
                            SelfLog.WriteLine("RichTextBox sink write failed: " + ex);
                            announcedFailure = true;
                        }

                        if (_shutdown.IsCancellationRequested)
                        {
                            SelfLog.WriteLine("RichTextBox sink dropped " + batchToWrite.Count + " lines during shutdown.");
                            break;
                        }

                        try
                        {
                            await Task.Delay(CoalesceWindow, _shutdown.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            SelfLog.WriteLine("RichTextBox sink dropped " + batchToWrite.Count + " lines during shutdown.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("RichTextBox sink consumer stopped: " + ex);
            }
        }

        private void FillBatch(List<RichTextLine> batch)
        {
            while (batch.Count < MaxLinesPerTurn && _messageReader.TryRead(out var logEvent))
            {
                batch.Add(FormatEvent(logEvent));
            }
        }

        private async Task CoalesceAsync(List<RichTextLine> batch)
        {
            using var timeout = new CancellationTokenSource(CoalesceWindow);
            while (batch.Count < MaxLinesPerTurn)
            {
                FillBatch(batch);
                if (batch.Count >= MaxLinesPerTurn)
                {
                    return;
                }

                try
                {
                    if (!await _messageReader.WaitToReadAsync(timeout.Token).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private RichTextLine FormatEvent(LogEvent logEvent)
        {
            try
            {
                return _formatter.FormatLine(logEvent);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("RichTextBox sink format failed: " + ex);
                return new RichTextLine(new List<RichTextFragment> { new(ex.Message, default) });
            }
        }

        public void Emit(LogEvent logEvent)
        {
            _messageWriter.TryWrite(logEvent);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeOnce, 1) != 0)
            {
                return;
            }

            _messageWriter.TryComplete();

            var consumer = _consumer;
            if (!consumer.IsCompleted)
            {
                if (_richTextBox.CheckAccess())
                {
                    PumpUntil(consumer, TimeSpan.FromSeconds(2));
                }
                else
                {
                    consumer.Wait(TimeSpan.FromSeconds(2));
                }
            }

            if (!consumer.IsCompleted)
            {
                _shutdown.Cancel();
                SelfLog.WriteLine("RichTextBox sink flush timed out.");
            }
        }

        private static void PumpUntil(Task consumer, TimeSpan timeout)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer(DispatcherPriority.Send, dispatcher)
            {
                Interval = timeout,
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                frame.Continue = false;
            };
            consumer.ContinueWith(
                _ => dispatcher.BeginInvoke(new Action(() =>
                {
                    timer.Stop();
                    frame.Continue = false;
                })),
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
            timer.Start();
            Dispatcher.PushFrame(frame);
        }
    }
}
