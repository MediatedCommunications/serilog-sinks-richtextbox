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

using System.Windows.Media;

namespace Serilog.Sinks.RichTextBox.Themes
{
    internal static class ConsoleHtmlColor
    {
        public static Brush Black { get; } = Solid(0x00, 0x00, 0x00);
        public static Brush DarkBlue { get; } = Solid(0x00, 0x00, 0x80);
        public static Brush DarkGreen { get; } = Solid(0x00, 0x80, 0x00);
        public static Brush DarkCyan { get; } = Solid(0x00, 0x80, 0x80);
        public static Brush DarkRed { get; } = Solid(0x80, 0x00, 0x00);
        public static Brush DarkMagenta { get; } = Solid(0x80, 0x00, 0x80);
        public static Brush DarkYellow { get; } = Solid(0x80, 0x80, 0x00);
        public static Brush Gray { get; } = Solid(0xc0, 0xc0, 0xc0);
        public static Brush DarkGray { get; } = Solid(0x80, 0x80, 0x80);
        public static Brush Blue { get; } = Solid(0x00, 0x00, 0xff);
        public static Brush Green { get; } = Solid(0x00, 0xff, 0x00);
        public static Brush Cyan { get; } = Solid(0x00, 0xff, 0xff);
        public static Brush Red { get; } = Solid(0xff, 0x00, 0x00);
        public static Brush Magenta { get; } = Solid(0xff, 0x00, 0xff);
        public static Brush Yellow { get; } = Solid(0xff, 0xff, 0x00);
        public static Brush White { get; } = Solid(0xff, 0xff, 0xff);

        private static SolidColorBrush Solid(byte red, byte green, byte blue)
        {
            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
