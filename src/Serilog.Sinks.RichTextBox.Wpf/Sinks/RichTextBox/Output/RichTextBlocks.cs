using System.Collections.Generic;
using System.Windows.Documents;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Output
{
    internal static class RichTextBlocks
    {
        public static List<Paragraph> FromLines(IReadOnlyList<RichTextLine> lines)
        {
            var paragraphs = new List<Paragraph>(lines?.Count ?? 0);
            if (lines == null)
            {
                return paragraphs;
            }

            foreach (var line in lines)
            {
                var paragraph = new Paragraph();
                foreach (var fragment in line.Fragments)
                {
                    if (string.IsNullOrEmpty(fragment.Text))
                    {
                        continue;
                    }

                    var run = new Run(fragment.Text);
                    if (fragment.Style.Foreground is { } foreground)
                    {
                        run.Foreground = foreground;
                    }

                    if (fragment.Style.Background is { } background)
                    {
                        run.Background = background;
                    }

                    if (fragment.Style.FontWeight is { } fontWeight)
                    {
                        run.FontWeight = fontWeight;
                    }

                    paragraph.Inlines.Add(run);
                }

                paragraphs.Add(paragraph);
            }

            return paragraphs;
        }
    }
}
