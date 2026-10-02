using System.Collections.Generic;
using System.Windows.Documents;

namespace Serilog.Sinks.RichTextBox.Output
{
    public interface IRichTextBoxOutputAppender
    {
        void Append(System.Windows.Controls.RichTextBox richTextBox, FlowDocument document, List<string> Paragraph);
    }

}
