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
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextBox.Formatting;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;

namespace Serilog.Sinks.RichTextBox.Output
{
    internal class PropertiesTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly MessageTemplate _outputTemplate;
        private readonly PropertyToken _token;
        private readonly ThemedValueFormatter _valueFormatter;

        public PropertiesTokenRenderer(RichTextBoxTheme theme, PropertyToken token, MessageTemplate outputTemplate, IFormatProvider formatProvider)
        {
            _outputTemplate = outputTemplate;
            if (theme is null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            _token = token ?? throw new ArgumentNullException(nameof(token));

            var isJson = false;

            if (token.Format != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < token.Format.Length; ++i)
                {
                    if (token.Format[i] == 'j')
                    {
                        isJson = true;
                    }
                }
            }

            _valueFormatter = isJson
                ? (ThemedValueFormatter)new ThemedJsonValueFormatter(theme, formatProvider)
                : new ThemedDisplayValueFormatter(theme, formatProvider);
        }

        public override void Render(LogEvent logEvent, RichTextLineWriter output)
        {
            var included = logEvent.Properties
                .Where(p => !TemplateContainsPropertyName(logEvent.MessageTemplate, p.Key) &&
                            !TemplateContainsPropertyName(_outputTemplate, p.Key))
                .Select(p => new LogEventProperty(p.Key, p.Value));

            var value = new StructureValue(included);

            if (_token.Alignment is null)
            {
                _valueFormatter.Format(value, output, null);
                return;
            }

            var buffer = new RichTextLineWriter();
            _valueFormatter.Format(value, buffer, null);
            output.WriteAligned(buffer.DetachFragments(), _token.Alignment);
        }

        private static bool TemplateContainsPropertyName(MessageTemplate template, string propertyName)
        {
            foreach (var token in template.Tokens)
            {
                if (token is PropertyToken namedProperty &&
                    namedProperty.PropertyName == propertyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
