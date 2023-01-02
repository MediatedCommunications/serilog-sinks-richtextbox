using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Serilog.Sinks.RichTextBox.Output
{
    public class ParagraphRichTextBoxOutputAppender : RichTextBoxOutputAppenderBase<ParagraphRichTextBoxOutputAppenderArgs>
    {

        public ParagraphRichTextBoxOutputAppender(ParagraphRichTextBoxOutputAppenderArgs args) : base(args)
        {

        }

        public override void Append(System.Windows.Controls.RichTextBox richTextBox, List<string> Paragraph)
        {
            var Text = Paragraph;

            if (Args.MaxItems is { } MaxItems && Text.Count > MaxItems) {
                var Skip = Paragraph.Count - MaxItems;
                Text = Text.Skip(Skip).ToList();
            }

            base.Append(richTextBox, Text);
        }

        private void Step1(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs) {
            foreach (var paragraph in paragraphs) {

                if (paragraph.Inlines.LastInline is Run { } Run && (Run.Text == Environment.NewLine || Run.Text == "\n")) {
                    paragraph.Inlines.Remove(Run);
                }
            }
        }

        private void Step2(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs) {
            if (Args.Prepend) {
                foreach (var paragraph in paragraphs) {
                    document.Blocks.InsertBefore(document.Blocks.FirstBlock, paragraph);
                }
            }
            else {
                document.Blocks.AddRange(paragraphs);
            }
        }

        private void Step3(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs) {

            {
                if (Args.MaxItems is { } MaxItems && MaxItems > 0) {
                    if (Args.Prepend) {
                        var FirstBlock = document.Blocks.Skip(MaxItems).FirstOrDefault();
                        if (FirstBlock is { }) {
                            var TR = new TextRange(FirstBlock.ContentStart, document.Blocks.LastBlock.ContentEnd);
                            TR.Text = string.Empty;
                        }
                    }
                    else {
                        var Skip = document.Blocks.Count - MaxItems;
                        var LastBlock = document.Blocks.Skip(Skip).FirstOrDefault();
                        if (LastBlock is { }) {
                            var TR = new TextRange(document.Blocks.FirstBlock.ContentStart, LastBlock.ContentEnd);
                            TR.Text = string.Empty;
                        }

                    }
                }
            }

            {
                if (Args.MaxItems is { } MaxItems && MaxItems > 0) {
                    while (document.Blocks.Count > MaxItems) {
                        if (Args.Prepend) {
                            document.Blocks.Remove(document.Blocks.LastBlock);
                        }
                        else {
                            document.Blocks.Remove(document.Blocks.FirstBlock);
                        }
                    }
                }
            }

        }

        private void Step4(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs) {
            try {
                if (Args.ScrollOnChange) {
                    if (Args.Prepend) {
                        richTextBox.ScrollToHome();
                    }
                    else {
                        richTextBox.ScrollToEnd();
                    }
                }
            } catch {
                
            }
        }

        protected override void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs)
        {

            richTextBox.BeginChange();
            
            {
                Step1(richTextBox, document, paragraphs);
            }

            {
                Step2(richTextBox, document, paragraphs);
            }

            {
                Step3(richTextBox, document, paragraphs);
            }

            {
                Step4(richTextBox, document, paragraphs);
            }

            richTextBox.EndChange();
        }

    }

}
