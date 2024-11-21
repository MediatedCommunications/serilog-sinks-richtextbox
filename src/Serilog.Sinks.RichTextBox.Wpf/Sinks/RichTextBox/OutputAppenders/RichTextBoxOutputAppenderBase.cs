using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Serilog.Sinks.RichTextBox.Output
{
    public abstract class RichTextBoxOutputAppenderBase : IRichTextBoxOutputAppender
    {

        public virtual void Append(System.Windows.Controls.RichTextBox richTextBox, List<string> Paragraph)
        {
            var flowDocument = richTextBox.Document ??= new FlowDocument();

            var SW = System.Diagnostics.Stopwatch.StartNew();
            var paragraphs = Parse(Paragraph);
            SW.Stop();

            Append(richTextBox, flowDocument, paragraphs);
        }

        protected abstract void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs);


        protected List<Paragraph> Parse(List<string> xamlParagraphTexts)
        {
            var ret = new List<Paragraph>();

            foreach (var xamlParagraphText in xamlParagraphTexts)
            {


                try
                {
                    var parsedParagraph = (Paragraph)XamlReader.Parse(xamlParagraphText);
                    ret.Add(parsedParagraph);
                } catch (Exception ex)
                {
                    var errorParagraph = new Paragraph() {
                        Inlines = {
                                new Run($"Error parsing `{xamlParagraphText}` to XAML: {ex.Message}")
                            }
                    };

                    ret.Add(errorParagraph);
                }
                //} catch (XamlParseException ex)
                //{
                //    SelfLog.WriteLine($"Error parsing `{xamlParagraphText}` to XAML: {ex.Message}");
                //    throw;
                //}
            }

            return ret;
        }
    }

}
