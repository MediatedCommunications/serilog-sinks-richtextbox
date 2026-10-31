#region Copyright 2021 C. Augusto Proiete & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Globalization;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;

namespace Serilog.Sinks.RichTextBox.Formatting
{
    internal class ThemedJsonValueFormatter : ThemedValueFormatter
    {
        private readonly ThemedDisplayValueFormatter _displayFormatter;
        private readonly IFormatProvider _formatProvider;

        public ThemedJsonValueFormatter(RichTextBoxTheme theme, IFormatProvider formatProvider)
            : base(theme)
        {
            _displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);
            _formatProvider = formatProvider;
        }

        protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
        {
            if (scalar is null)
            {
                throw new ArgumentNullException(nameof(scalar));
            }

            // At the top level, for scalar values, use "display" rendering.
            if (state.IsTopLevel)
            {
                return _displayFormatter.FormatLiteralValue(scalar, state.Output, state.Format);
            }

            return FormatLiteralValue(scalar, state.Output);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence is null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('[');
            }

            var delim = string.Empty;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < sequence.Elements.Count; ++index)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                    {
                        state.Output.Write(delim);
                    }
                }

                delim = ", ";
                Visit(state.Nest(), sequence.Elements[index]);
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write(']');
            }

            return 0;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('{');
            }

            var delim = string.Empty;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < structure.Properties.Count; ++index)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                    {
                        state.Output.Write(delim);
                    }
                }

                delim = ", ";

                var property = structure.Properties[index];

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.Name))
                {
                    state.Output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(property.Name, writer));
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write(": ");
                }

                Visit(state.Nest(), property.Value);
            }

            if (structure.TypeTag != null)
            {
                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write(delim);
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.Name))
                {
                    state.Output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString("$type", writer));
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write(": ");
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.String))
                {
                    var typeTag = structure.TypeTag;
                    state.Output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(typeTag, writer));
                }
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('}');
            }

            return 0;
        }

        protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
        {
            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('{');
            }

            var delim = string.Empty;
            foreach (var element in dictionary.Elements)
            {
                if (delim.Length != 0)
                {
                    using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                    {
                        state.Output.Write(delim);
                    }
                }

                delim = ", ";

                var style = element.Key.Value is null
                    ? RichTextBoxThemeStyle.Null
                    : element.Key.Value is string
                        ? RichTextBoxThemeStyle.String
                        : RichTextBoxThemeStyle.Scalar;

                using (ApplyStyle(state.Output, style))
                {
                    var key = (element.Key.Value ?? "null").ToString();
                    state.Output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(key, writer));
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write(": ");
                }

                Visit(state.Nest(), element.Value);
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('}');
            }

            return 0;
        }

        private int FormatLiteralValue(ScalarValue scalar, RichTextLineWriter output)
        {
            var value = scalar.Value;

            if (value is null)
            {
                using (ApplyStyle(output, RichTextBoxThemeStyle.Null))
                {
                    output.Write("null");
                }

                return 0;
            }

            if (value is string str)
            {
                using (ApplyStyle(output, RichTextBoxThemeStyle.String))
                {
                    output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(str, writer));
                }

                return 0;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong || value is decimal ||
                    value is byte || value is sbyte || value is short || value is ushort)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Number))
                    {
                        output.Write(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
                    }

                    return 0;
                }

                if (value is double d)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Number))
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d))
                        {
                            var text = d.ToString(CultureInfo.InvariantCulture);
                            output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(text, writer));
                        }
                        else
                        {
                            output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                        }
                    }

                    return 0;
                }

                if (value is float f)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Number))
                    {
                        if (double.IsNaN(f) || double.IsInfinity(f))
                        {
                            var text = f.ToString(CultureInfo.InvariantCulture);
                            output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(text, writer));
                        }
                        else
                        {
                            output.Write(f.ToString("R", CultureInfo.InvariantCulture));
                        }
                    }

                    return 0;
                }

                if (value is bool b)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Boolean))
                    {
                        output.Write(b ? "true" : "false");
                    }

                    return 0;
                }

                if (value is char ch)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Scalar))
                    {
                        var charString = ch.ToString();
                        output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(charString, writer));
                    }

                    return 0;
                }

                if (value is DateTime || value is DateTimeOffset)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Scalar))
                    {
                        output.Write('\"');
                        output.Write(((IFormattable)value).ToString("O", CultureInfo.InvariantCulture));
                        output.Write('\"');
                    }

                    return 0;
                }
            }

            using (ApplyStyle(output, RichTextBoxThemeStyle.Scalar))
            {
                var text = value.ToString();
                output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(text, writer));
            }

            return 0;
        }
    }
}
