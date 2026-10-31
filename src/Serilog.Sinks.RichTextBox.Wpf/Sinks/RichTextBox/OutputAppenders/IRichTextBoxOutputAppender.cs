using System.Collections.Generic;
using System.Windows.Documents;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Output
{
    public interface IRichTextBoxOutputAppender
    {
        void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, IReadOnlyList<RichTextLine> lines);
    }
}
