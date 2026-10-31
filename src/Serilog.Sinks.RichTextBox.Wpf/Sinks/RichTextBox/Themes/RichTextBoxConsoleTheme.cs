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
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Themes
{
    /// <summary>
    /// A RichTextBox theme using the styling facilities of the <see cref="System.Windows.Documents.Run"/> class.
    /// </summary>
    public class RichTextBoxConsoleTheme : RichTextBoxTheme
    {
        private readonly Dictionary<RichTextBoxThemeStyle, RichTextRunStyle> _directStyles;

        /// <summary>
        /// Construct a theme given a set of styles.
        /// </summary>
        /// <param name="styles">Styles to apply within the theme.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="styles"/> is <code>null</code></exception>
        public RichTextBoxConsoleTheme(IReadOnlyDictionary<RichTextBoxThemeStyle, RichTextBoxConsoleThemeStyle> styles)
        {
            if (styles is null)
            {
                throw new ArgumentNullException(nameof(styles));
            }

            _directStyles = new Dictionary<RichTextBoxThemeStyle, RichTextRunStyle>(styles.Count);
            foreach (var pair in styles)
            {
                _directStyles.Add(pair.Key, ToDirectStyle(pair.Value));
            }
        }

        internal override RichTextRunStyle GetDirectStyle(RichTextBoxThemeStyle style)
        {
            return _directStyles.TryGetValue(style, out var direct) ? direct : default;
        }

        /// <summary>
        /// A theme in the style of the original <i>Serilog.Sinks.Literate</i>. This is the default when no theme is specified.
        /// </summary>
        public static RichTextBoxConsoleTheme Literate { get; } = RichTextBoxConsoleThemes.Literate;

        /// <summary>
        /// A theme using only gray, black and white.
        /// </summary>
        public static RichTextBoxConsoleTheme Grayscale { get; } = RichTextBoxConsoleThemes.Grayscale;

        /// <summary>
        /// A theme based on the original Serilog "colored console" sink.
        /// </summary>
        public static RichTextBoxConsoleTheme Colored { get; } = RichTextBoxConsoleThemes.Colored;

        private static RichTextRunStyle ToDirectStyle(RichTextBoxConsoleThemeStyle style)
        {
            return new RichTextRunStyle
            {
                Foreground = style.Foreground,
                Background = style.Background,
                FontWeight = style.FontWeight == default ? null : style.FontWeight,
            };
        }
    }
}
