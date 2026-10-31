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
    internal class ThemedDisplayValueFormatter : ThemedValueFormatter
    {
        private readonly IFormatProvider _formatProvider;

        public ThemedDisplayValueFormatter(RichTextBoxTheme theme, IFormatProvider formatProvider)
            : base(theme)
        {
            _formatProvider = formatProvider;
        }

        protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
        {
            if (scalar is null)
            {
                throw new ArgumentNullException(nameof(scalar));
            }

            return FormatLiteralValue(scalar, state.Output, state.Format);
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
                Visit(state, sequence.Elements[index]);
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write(']');
            }

            return 0;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            if (structure.TypeTag != null)
            {
                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.Name))
                {
                    state.Output.Write(structure.TypeTag);
                }

                state.Output.Write(' ');
            }

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
                    state.Output.Write(property.Name);
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write('=');
                }

                Visit(state.Nest(), property.Value);
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

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write('[');
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.String))
                {
                    Visit(state.Nest(), element.Key);
                }

                using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
                {
                    state.Output.Write("]=");
                }

                Visit(state.Nest(), element.Value);
            }

            using (ApplyStyle(state.Output, RichTextBoxThemeStyle.TertiaryText))
            {
                state.Output.Write('}');
            }

            return 0;
        }

        public int FormatLiteralValue(ScalarValue scalar, RichTextLineWriter output, string format)
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
                    if (format != "l")
                    {
                        output.WriteCaptured(writer => JsonValueFormatter.WriteQuotedJsonString(str, writer));
                    }
                    else
                    {
                        output.Write(str);
                    }
                }

                return 0;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong ||
                    value is decimal || value is byte || value is sbyte || value is short ||
                    value is ushort || value is float || value is double)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Number))
                    {
                        output.WriteCaptured(writer => scalar.Render(writer, format, _formatProvider));
                    }

                    return 0;
                }

                if (value is bool b)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Boolean))
                    {
                        output.Write(b.ToString(CultureInfo.InvariantCulture));
                    }

                    return 0;
                }

                if (value is char ch)
                {
                    using (ApplyStyle(output, RichTextBoxThemeStyle.Scalar))
                    {
                        output.Write('\'');
                        output.Write(ch.ToString());
                        output.Write('\'');
                    }

                    return 0;
                }
            }

            using (ApplyStyle(output, RichTextBoxThemeStyle.Scalar))
            {
                output.WriteCaptured(writer => scalar.Render(writer, format, _formatProvider));
            }

            return 0;
        }
    }
}
