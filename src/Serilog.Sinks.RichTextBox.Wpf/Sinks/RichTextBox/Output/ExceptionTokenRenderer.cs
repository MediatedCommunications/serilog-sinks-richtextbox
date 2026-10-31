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

using System.IO;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;

namespace Serilog.Sinks.RichTextBox.Output
{
    internal class ExceptionTokenRenderer : OutputTemplateTokenRenderer
    {
        private const string _stackFrameLinePrefix = "   ";

        private readonly RichTextBoxTheme _theme;

        public ExceptionTokenRenderer(RichTextBoxTheme theme)
        {
            _theme = theme;
        }

        public override void Render(LogEvent logEvent, RichTextLineWriter output)
        {
            // Padding is never applied by this renderer.

            if (logEvent.Exception is null)
            {
                return;
            }

            var logContent = logEvent.Exception.Message;
            try
            {
                logContent = logEvent.Exception.ToString();
            } catch { }

            var lines = new StringReader(logContent);

            string pending = null;
            string nextLine;
            while ((nextLine = lines.ReadLine()) != null)
            {
                if (pending != null)
                {
                    WriteExceptionLine(output, pending, includeNewline: true);
                }

                pending = nextLine;
            }

            if (pending != null)
            {
                WriteExceptionLine(output, pending, includeNewline: false);
            }
        }

        private void WriteExceptionLine(RichTextLineWriter output, string line, bool includeNewline)
        {
            var style = line.StartsWith(_stackFrameLinePrefix) ? RichTextBoxThemeStyle.SecondaryText : RichTextBoxThemeStyle.Text;

            using (_theme.Apply(output, style))
            {
                if (includeNewline)
                {
                    output.WriteLine(line);
                }
                else
                {
                    output.Write(line);
                }
            }
        }
    }
}
