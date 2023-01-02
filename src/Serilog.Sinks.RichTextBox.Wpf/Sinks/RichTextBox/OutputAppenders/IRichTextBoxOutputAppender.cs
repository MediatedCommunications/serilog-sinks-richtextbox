using System.Collections.Generic;

namespace Serilog.Sinks.RichTextBox.Output
{
    public interface IRichTextBoxOutputAppender
    {
        void Append(System.Windows.Controls.RichTextBox RichTextBox, List<string> Paragraph);
    }

}
