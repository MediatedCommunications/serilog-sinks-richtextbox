using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Output
{
    public class ParagraphRichTextBoxOutputAppender : RichTextBoxOutputAppenderBase
    {
        public ParagraphRichTextBoxOutputAppender(ParagraphRichTextBoxOutputAppenderArgs args)
        {
            Args = args;
        }

        protected ParagraphRichTextBoxOutputAppenderArgs Args { get; }

        private static readonly object _placeholderSentinel = new();

        private readonly ConditionalWeakTable<FlowDocument, object> _droppedPlaceholder = new();

        internal int? RetainedLineLimit => Args.MaxItems is int max && max > 0 ? max : null;

        private static void StripTrailingNewlines(List<Paragraph> paragraphs)
        {
            foreach (var paragraph in paragraphs)
            {
                while (paragraph.Inlines.LastInline is Run run)
                {
                    var text = run.Text ?? string.Empty;
                    var end = text.Length;
                    while (end > 0 && (text[end - 1] == '\n' || text[end - 1] == '\r'))
                    {
                        end--;
                    }

                    if (end == text.Length)
                    {
                        break;
                    }

                    if (end == 0)
                    {
                        paragraph.Inlines.Remove(run);
                        continue;
                    }

                    run.Text = text.Substring(0, end);
                    break;
                }
            }
        }

        protected override void Append(System.Windows.Controls.RichTextBox? richTextBox, FlowDocument document, List<Paragraph> paragraphs)
        {
            StripTrailingNewlines(paragraphs);

            var follow = false;
            if (Args.ScrollOnChange && richTextBox != null)
            {
                follow = Args.Prepend ? IsScrolledToHome(richTextBox) : IsScrolledToEnd(richTextBox);
            }

            var started = false;
            try
            {
                if (richTextBox != null)
                {
                    richTextBox.BeginChange();
                    started = true;
                }

                ApplyBlocks(document, paragraphs);
            }
            finally
            {
                if (started && richTextBox != null)
                {
                    richTextBox.EndChange();
                }
            }

            if (!follow || richTextBox == null)
            {
                return;
            }

            Follow(richTextBox);
        }

        private void ApplyBlocks(FlowDocument document, List<Paragraph> paragraphs)
        {
            DropLeadingPlaceholder(document);

            var ordered = Args.Prepend ? Reverse(paragraphs) : paragraphs;
            var max = Args.MaxItems is int limit && limit > 0 ? limit : 0;
            if (max > 0 && ordered.Count > max)
            {
                ordered = Args.Prepend
                    ? ordered.GetRange(0, max)
                    : ordered.GetRange(ordered.Count - max, max);
            }

            if (max > 0 && ordered.Count >= max)
            {
                document.Blocks.Clear();
                document.Blocks.AddRange(ordered);
                return;
            }

            if (Args.Prepend)
            {
                InsertAtStart(document, ordered);
            }
            else
            {
                document.Blocks.AddRange(ordered);
            }

            if (max > 0 && document.Blocks.Count > max)
            {
                if (Args.Prepend)
                {
                    RemoveTrailingBlocks(document, max);
                }
                else
                {
                    RemoveLeadingBlocks(document, document.Blocks.Count - max);
                }
            }
        }

        private static void InsertAtStart(FlowDocument document, List<Paragraph> paragraphs)
        {
            var anchor = document.Blocks.FirstBlock;
            if (anchor == null)
            {
                document.Blocks.AddRange(paragraphs);
                return;
            }

            foreach (var paragraph in paragraphs)
            {
                document.Blocks.InsertBefore(anchor, paragraph);
            }
        }

        private static List<Paragraph> Reverse(List<Paragraph> paragraphs)
        {
            var reversed = new List<Paragraph>(paragraphs.Count);
            for (var i = paragraphs.Count - 1; i >= 0; i--)
            {
                reversed.Add(paragraphs[i]);
            }

            return reversed;
        }

        private void DropLeadingPlaceholder(FlowDocument document)
        {
            if (_droppedPlaceholder.TryGetValue(document, out _))
            {
                return;
            }

            _droppedPlaceholder.Add(document, _placeholderSentinel);

            if (document.Blocks.Count == 1 &&
                document.Blocks.FirstBlock is Paragraph paragraph &&
                paragraph.Inlines.Count == 0)
            {
                document.Blocks.Clear();
            }
        }

        private void Follow(System.Windows.Controls.RichTextBox richTextBox)
        {
            try
            {
                richTextBox.UpdateLayout();
                if (richTextBox.ViewportHeight == 0 && richTextBox.ExtentHeight == 0)
                {
                    EventHandler? onLayout = null;
                    onLayout = (_, _) =>
                    {
                        if (richTextBox.ViewportHeight == 0 && richTextBox.ExtentHeight == 0)
                        {
                            return;
                        }

                        richTextBox.LayoutUpdated -= onLayout;
                        ScrollToFollow(richTextBox);
                    };
                    richTextBox.LayoutUpdated += onLayout;
                    return;
                }

                ScrollToFollow(richTextBox);
            }
            catch
            {
            }
        }

        private void ScrollToFollow(System.Windows.Controls.RichTextBox richTextBox)
        {
            try
            {
                if (Args.Prepend)
                {
                    richTextBox.ScrollToHome();
                }
                else
                {
                    richTextBox.ScrollToEnd();
                }
            }
            catch
            {
            }
        }

        private static void RemoveLeadingBlocks(FlowDocument document, int count)
        {
            var block = document.Blocks.FirstBlock;
            for (var i = 0; i < count && block != null; i++)
            {
                var next = block.NextBlock;
                document.Blocks.Remove(block);
                block = next;
            }
        }

        private static void RemoveTrailingBlocks(FlowDocument document, int keep)
        {
            var drop = BlockAt(document, keep);
            while (drop != null)
            {
                var next = drop.NextBlock;
                document.Blocks.Remove(drop);
                drop = next;
            }
        }

        private static Block? BlockAt(FlowDocument document, int index)
        {
            var block = document.Blocks.FirstBlock;
            for (var i = 0; i < index && block != null; i++)
            {
                block = block.NextBlock;
            }

            return block;
        }

        private static bool IsScrolledToEnd(System.Windows.Controls.RichTextBox richTextBox)
        {
            if (!richTextBox.Selection.IsEmpty)
            {
                return false;
            }

            return richTextBox.VerticalOffset + richTextBox.ViewportHeight >= richTextBox.ExtentHeight - 1d;
        }

        private static bool IsScrolledToHome(System.Windows.Controls.RichTextBox richTextBox)
        {
            if (!richTextBox.Selection.IsEmpty)
            {
                return false;
            }

            return richTextBox.VerticalOffset <= 1d;
        }
    }
}
