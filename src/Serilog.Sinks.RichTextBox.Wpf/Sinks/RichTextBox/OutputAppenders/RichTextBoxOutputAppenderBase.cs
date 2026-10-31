using System.Collections.Generic;
using System.Windows.Documents;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Output
{
    public abstract class RichTextBoxOutputAppenderBase : IRichTextBoxOutputAppender
    {
        public virtual void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, IReadOnlyList<RichTextLine> lines)
        {
            Append(richTextBox, document, RichTextBlocks.FromLines(lines));
        }

        protected abstract void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<Paragraph> paragraphs);
    }
}
