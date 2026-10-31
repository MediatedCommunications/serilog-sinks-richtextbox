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

using System.Windows;
using System.Windows.Media;

namespace Serilog.Sinks.RichTextBox.Themes
{
    /// <summary>
    /// Styling applied for foreground, background, and font weight.
    /// </summary>
    public struct RichTextBoxConsoleThemeStyle
    {
        /// <summary>
        /// The background brush to apply.
        /// </summary>
        public Brush Background;

        /// <summary>
        /// The foreground brush to apply.
        /// </summary>
        public Brush Foreground;

        /// <summary>
        /// The font weight to apply.
        /// </summary>
        public FontWeight FontWeight;
    }
}
