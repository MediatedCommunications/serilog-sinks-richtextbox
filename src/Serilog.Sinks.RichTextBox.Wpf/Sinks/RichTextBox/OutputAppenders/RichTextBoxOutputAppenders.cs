namespace Serilog.Sinks.RichTextBox.Output
{
    public static class RichTextBoxOutputAppenders
    {
        public static IRichTextBoxOutputAppender Default { get; } = new ParagraphRichTextBoxOutputAppender(new ParagraphRichTextBoxOutputAppenderArgs
        {
            ScrollOnChange = true,
            MaxItems = 100,
        });
    }
}
