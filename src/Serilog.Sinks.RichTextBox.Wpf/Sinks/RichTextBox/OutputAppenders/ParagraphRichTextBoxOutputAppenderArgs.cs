namespace Serilog.Sinks.RichTextBox.Output
{
    public record ParagraphRichTextBoxOutputAppenderArgs
    {
        public bool ScrollOnChange { get; init; }
        public bool Prepend { get; init; }
        public int? MaxItems { get; init; }
    }

}
