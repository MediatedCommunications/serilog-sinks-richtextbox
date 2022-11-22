using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Serilog.Sinks.RichTextBox.Output
{
    public class ParagraphRichTextBoxOutputAppender : RichTextBoxOutputAppenderBase<ParagraphRichTextBoxOutputAppenderArgs>
    {

        public ParagraphRichTextBoxOutputAppender(ParagraphRichTextBoxOutputAppenderArgs args) : base(args)
        {

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
            if (Args.MaxItems is { } Trim && Trim > 0) {
                while (document.Blocks.Count > Trim) {
                    if (Args.Prepend) {
                        document.Blocks.Remove(document.Blocks.LastBlock);
                    }
                    else {
                        document.Blocks.Remove(document.Blocks.FirstBlock);
                    }
                }
            }
        }

        private void Step4(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs) {
            if (Args.ScrollOnChange) {
                if (Args.Prepend) {
                    richTextBox.ScrollToHome();
                }
                else {
                    richTextBox.ScrollToEnd();
                }
            }
        }

        protected override void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs)
        {
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

        }

    }

}
