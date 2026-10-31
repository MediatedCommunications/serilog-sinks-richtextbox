using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog.Parsing;

namespace Serilog.Sinks.RichTextBox.Rendering
{
    internal sealed class RichTextLineWriter
    {
        private readonly Stack<RichTextRunStyle> _styles = new();
        private readonly List<RichTextFragment> _fragments = new();
        private readonly StringBuilder _text = new();

        public void Push(RichTextRunStyle style)
        {
            Commit();
            _styles.Push(style);
        }

        public void Pop()
        {
            Commit();
            if (_styles.Count > 0)
            {
                _styles.Pop();
            }
        }

        public void Write(char value)
        {
            _text.Append(value);
        }

        public void Write(string? value)
        {
            if (value != null)
            {
                _text.Append(value);
            }
        }

        public void Write(char[] buffer, int index, int count)
        {
            _text.Append(buffer, index, count);
        }

        public void WriteLine()
        {
            _text.Append(Environment.NewLine);
        }

        public void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteCaptured(Action<TextWriter> write)
        {
            Write(Capture(write));
        }

        public static string Capture(Action<TextWriter> write)
        {
            var buffer = new StringBuilder();
            write(new CapturingWriter(buffer));
            return buffer.ToString();
        }

        public RichTextLine ToLine()
        {
            return new RichTextLine(DetachFragments());
        }

        public List<RichTextFragment> DetachFragments()
        {
            Commit();
            if (_fragments.Count == 0)
            {
                return _fragments;
            }

            var detached = new List<RichTextFragment>(_fragments);
            _fragments.Clear();
            return detached;
        }

        public int VisibleLength
        {
            get
            {
                Commit();
                var length = 0;
                foreach (var fragment in _fragments)
                {
                    length += fragment.Text.Length;
                }

                return length;
            }
        }

        public void WriteFragments(IReadOnlyList<RichTextFragment> fragments)
        {
            Commit();
            foreach (var fragment in fragments)
            {
                if (fragment.Text.Length > 0)
                {
                    _fragments.Add(fragment);
                }
            }
        }

        public void WriteAligned(IReadOnlyList<RichTextFragment> fragments, Alignment? alignment)
        {
            var visible = 0;
            foreach (var fragment in fragments)
            {
                visible += fragment.Text?.Length ?? 0;
            }

            if (alignment is null || visible >= alignment.Value.Width)
            {
                WriteFragments(fragments);
                return;
            }

            var pad = alignment.Value.Width - visible;
            var spaces = new string(' ', pad);
            if (alignment.Value.Direction == AlignmentDirection.Right)
            {
                Write(spaces);
            }

            WriteFragments(fragments);

            if (alignment.Value.Direction == AlignmentDirection.Left)
            {
                Write(spaces);
            }

            Commit();
        }

        public override string ToString()
        {
            Commit();
            if (_fragments.Count == 0)
            {
                return string.Empty;
            }

            if (_fragments.Count == 1)
            {
                return _fragments[0].Text;
            }

            var text = new StringBuilder();
            foreach (var fragment in _fragments)
            {
                text.Append(fragment.Text);
            }

            return text.ToString();
        }

        private void Commit()
        {
            if (_text.Length == 0)
            {
                return;
            }

            var style = _styles.Count > 0 ? _styles.Peek() : default;
            _fragments.Add(new RichTextFragment(_text.ToString(), style));
            _text.Clear();
        }

        private sealed class CapturingWriter : TextWriter
        {
            private readonly StringBuilder _buffer;

            public CapturingWriter(StringBuilder buffer)
            {
                _buffer = buffer;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                _buffer.Append(value);
            }

            public override void Write(string? value)
            {
                if (value != null)
                {
                    _buffer.Append(value);
                }
            }

            public override void Write(char[] buffer, int index, int count)
            {
                _buffer.Append(buffer, index, count);
            }
        }
    }
}
