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
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;

namespace Serilog.Sinks.RichTextBox.Output
{
    internal class EventPropertyTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly RichTextBoxTheme _theme;
        private readonly PropertyToken _token;
        private readonly IFormatProvider _formatProvider;

        public EventPropertyTokenRenderer(RichTextBoxTheme theme, PropertyToken token, IFormatProvider formatProvider)
        {
            _theme = theme;
            _token = token;
            _formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, RichTextLineWriter output)
        {
            // If a property is missing, don't render anything (message templates render the raw token here).
            if (!logEvent.Properties.TryGetValue(_token.PropertyName, out var propertyValue))
            {
                Padding.Apply(output, string.Empty, _token.Alignment);
                return;
            }

            using (_theme.Apply(output, RichTextBoxThemeStyle.SecondaryText))
            {
                string rendered;
                if (propertyValue is ScalarValue { Value: string literalString })
                {
                    rendered = Casing.Format(literalString, _token.Format);
                }
                else
                {
                    rendered = RichTextLineWriter.Capture(writer => propertyValue.Render(writer, _token.Format, _formatProvider));
                }

                Padding.Apply(output, rendered, _token.Alignment);
            }
        }
    }
}
