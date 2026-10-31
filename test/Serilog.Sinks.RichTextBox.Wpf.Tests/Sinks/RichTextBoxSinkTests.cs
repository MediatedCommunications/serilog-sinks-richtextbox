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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Sinks.RichTextBox.Abstraction;
using Serilog.Sinks.RichTextBox.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Serilog.Sinks.RichTextBox.Wpf.Tests.Sinks
{
    [TestClass]
    public class RichTextBoxSinkTests
    {
        [TestMethod]
        public void NullRichTextBoxThrows()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new LoggerConfiguration().WriteTo.RichTextBox((System.Windows.Controls.RichTextBox)null!));

            Assert.AreEqual("richTextBoxControl", exception.ParamName);
        }

        [TestMethod]
        public void FailedWriteIsRetriedBeforeFlushReturns()
        {
            var box = new RetryRichTextBox(failures: 2);
            var logger = new LoggerConfiguration()
                .WriteTo.RichTextBox(box)
                .CreateLogger();

            logger.Information("kept");
            logger.Dispose();

            Assert.Contains("kept", box.Content);
        }

        private sealed class RetryRichTextBox : IRichTextBox
        {
            private int _remainingFailures;
            private readonly StringBuilder _content = new();

            public RetryRichTextBox(int failures)
            {
                _remainingFailures = failures;
            }

            public string Content => _content.ToString();

            public bool CheckAccess() => false;

            public Task WriteAsync(IReadOnlyList<RichTextLine> lines)
            {
                if (Interlocked.Decrement(ref _remainingFailures) >= 0)
                {
                    throw new InvalidOperationException("busy");
                }

                foreach (var line in lines)
                {
                    foreach (var fragment in line.Fragments)
                    {
                        _content.Append(fragment.Text);
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
