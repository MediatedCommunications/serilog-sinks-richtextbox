using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Serilog.Sinks.RichTextBox.Rendering
{
    public readonly record struct RichTextRunStyle
    {
        public Brush Foreground { get; init; }

        public Brush Background { get; init; }

        public FontWeight? FontWeight { get; init; }
    }

    public readonly record struct RichTextFragment(string Text, RichTextRunStyle Style);

    public sealed class RichTextLine
    {
        public RichTextLine(List<RichTextFragment> fragments)
        {
            Fragments = fragments;
        }

        public List<RichTextFragment> Fragments { get; }
    }
}
