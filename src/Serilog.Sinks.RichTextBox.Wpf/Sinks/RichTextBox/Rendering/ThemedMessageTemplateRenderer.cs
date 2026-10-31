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
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextBox.Formatting;
using Serilog.Sinks.RichTextBox.Themes;

namespace Serilog.Sinks.RichTextBox.Rendering
{
    internal class ThemedMessageTemplateRenderer
    {
        private readonly RichTextBoxTheme _theme;
        private readonly ThemedValueFormatter _valueFormatter;
        private readonly bool _isLiteral;

        public ThemedMessageTemplateRenderer(RichTextBoxTheme theme, ThemedValueFormatter valueFormatter, bool isLiteral)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _valueFormatter = valueFormatter;
            _isLiteral = isLiteral;
        }

        public void Render(MessageTemplate template, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextLineWriter output)
        {
            foreach (var token in template.Tokens)
            {
                if (token is TextToken tt)
                {
                    RenderTextToken(tt, output);
                }
                else
                {
                    RenderPropertyToken((PropertyToken)token, properties, output);
                }
            }
        }

        private void RenderTextToken(TextToken tt, RichTextLineWriter output)
        {
            using (_theme.Apply(output, RichTextBoxThemeStyle.Text))
            {
                output.Write(tt.Text);
            }
        }

        private void RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextLineWriter output)
        {
            if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
            {
                using (_theme.Apply(output, RichTextBoxThemeStyle.Invalid))
                {
                    output.Write(pt.ToString());
                }

                return;
            }

            if (!pt.Alignment.HasValue)
            {
                RenderValue(_theme, _valueFormatter, propertyValue, output, pt.Format);
                return;
            }

            var valueOutput = new RichTextLineWriter();
            RenderValue(_theme, _valueFormatter, propertyValue, valueOutput, pt.Format);
            output.WriteAligned(valueOutput.DetachFragments(), pt.Alignment);
        }

        private void RenderValue(RichTextBoxTheme theme, ThemedValueFormatter valueFormatter, LogEventPropertyValue propertyValue, RichTextLineWriter output, string? format)
        {
            if (_isLiteral && propertyValue is ScalarValue { Value: string } sv)
            {
                using (theme.Apply(output, RichTextBoxThemeStyle.String))
                {
                    output.Write(sv.Value.ToString());
                }

                return;
            }

            valueFormatter.Format(propertyValue, output, format, _isLiteral);
        }
    }
}
