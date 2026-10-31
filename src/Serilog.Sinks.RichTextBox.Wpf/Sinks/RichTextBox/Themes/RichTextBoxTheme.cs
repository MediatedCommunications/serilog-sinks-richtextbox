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

using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Themes
{
    /// <summary>
    /// The base class for styled RichTextBox output.
    /// </summary>
    public abstract class RichTextBoxTheme
    {
        /// <summary>
        /// No styling applied.
        /// </summary>
        public static RichTextBoxTheme None { get; } = new EmptyRichTextBoxTheme();

        internal virtual RichTextRunStyle GetDirectStyle(RichTextBoxThemeStyle style)
        {
            return default;
        }

        internal StyleReset Apply(RichTextLineWriter output, RichTextBoxThemeStyle style)
        {
            output.Push(GetDirectStyle(style));
            return new StyleReset(output);
        }
    }
}
