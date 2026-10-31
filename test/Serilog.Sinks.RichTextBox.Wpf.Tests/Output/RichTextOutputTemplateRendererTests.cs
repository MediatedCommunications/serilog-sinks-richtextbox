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
using System.Windows.Media;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Output;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;
using Serilog.Sinks.RichTextBox.Wpf.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Serilog.Sinks.RichTextBox.Wpf.Tests.Output
{
    [TestClass]
    public class RichTextOutputTemplateRendererTests
    {
        [TestMethod]
        public void UsesFormatProvider()
        {
            var french = new CultureInfo("fr-FR");
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message}", french);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("{0}", 12.345));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("12,345", sw.ToString());
        }

        [TestMethod]
        public void MessageTemplatesContainingFormatStringPropertiesRenderCorrectly()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("{Message}", "Hello, world!"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("\"Hello, world!\"", sw.ToString());
        }

        [TestMethod]
        public void UppercaseFormatSpecifierIsSupportedForStrings()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Name:u}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("{Name}", "Nick"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("NICK", sw.ToString());
        }

        [TestMethod]
        public void LowercaseFormatSpecifierIsSupportedForStrings()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Name:w}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("{Name}", "Nick"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("nick", sw.ToString());
        }

        [TestMethod]
        [DataRow(LogEventLevel.Verbose, 1, "V")]
        [DataRow(LogEventLevel.Verbose, 2, "Vb")]
        [DataRow(LogEventLevel.Verbose, 3, "Vrb")]
        [DataRow(LogEventLevel.Verbose, 4, "Verb")]
        [DataRow(LogEventLevel.Verbose, 5, "Verbo")]
        [DataRow(LogEventLevel.Verbose, 6, "Verbos")]
        [DataRow(LogEventLevel.Verbose, 7, "Verbose")]
        [DataRow(LogEventLevel.Verbose, 8, "Verbose")]
        [DataRow(LogEventLevel.Debug, 1, "D")]
        [DataRow(LogEventLevel.Debug, 2, "De")]
        [DataRow(LogEventLevel.Debug, 3, "Dbg")]
        [DataRow(LogEventLevel.Debug, 4, "Dbug")]
        [DataRow(LogEventLevel.Debug, 5, "Debug")]
        [DataRow(LogEventLevel.Debug, 6, "Debug")]
        [DataRow(LogEventLevel.Information, 1, "I")]
        [DataRow(LogEventLevel.Information, 2, "In")]
        [DataRow(LogEventLevel.Information, 3, "Inf")]
        [DataRow(LogEventLevel.Information, 4, "Info")]
        [DataRow(LogEventLevel.Information, 5, "Infor")]
        [DataRow(LogEventLevel.Information, 6, "Inform")]
        [DataRow(LogEventLevel.Information, 7, "Informa")]
        [DataRow(LogEventLevel.Information, 8, "Informat")]
        [DataRow(LogEventLevel.Information, 9, "Informati")]
        [DataRow(LogEventLevel.Information, 10, "Informatio")]
        [DataRow(LogEventLevel.Information, 11, "Information")]
        [DataRow(LogEventLevel.Information, 12, "Information")]
        [DataRow(LogEventLevel.Error, 1, "E")]
        [DataRow(LogEventLevel.Error, 2, "Er")]
        [DataRow(LogEventLevel.Error, 3, "Err")]
        [DataRow(LogEventLevel.Error, 4, "Eror")]
        [DataRow(LogEventLevel.Error, 5, "Error")]
        [DataRow(LogEventLevel.Error, 6, "Error")]
        [DataRow(LogEventLevel.Fatal, 1, "F")]
        [DataRow(LogEventLevel.Fatal, 2, "Fa")]
        [DataRow(LogEventLevel.Fatal, 3, "Ftl")]
        [DataRow(LogEventLevel.Fatal, 4, "Fatl")]
        [DataRow(LogEventLevel.Fatal, 5, "Fatal")]
        [DataRow(LogEventLevel.Fatal, 6, "Fatal")]
        [DataRow(LogEventLevel.Warning, 1, "W")]
        [DataRow(LogEventLevel.Warning, 2, "Wn")]
        [DataRow(LogEventLevel.Warning, 3, "Wrn")]
        [DataRow(LogEventLevel.Warning, 4, "Warn")]
        [DataRow(LogEventLevel.Warning, 5, "Warni")]
        [DataRow(LogEventLevel.Warning, 6, "Warnin")]
        [DataRow(LogEventLevel.Warning, 7, "Warning")]
        [DataRow(LogEventLevel.Warning, 8, "Warning")]
        public void FixedLengthLevelIsSupported(
            LogEventLevel level,
            int width,
            string expected)
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, $"{{Level:t{width}}}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Write(level, "Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual(expected, sw.ToString());
        }

        [TestMethod]
        public void FixedLengthLevelSupportsUpperCasing()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Level:u3}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("INF", sw.ToString());
        }

        [TestMethod]
        public void FixedLengthLevelSupportsLowerCasing()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Level:w3}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("inf", sw.ToString());
        }

        [TestMethod]
        public void DefaultLevelLengthIsFullText()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Level}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("Information", sw.ToString());
        }

        [TestMethod]
        public void AlignmentAndWidthCanBeCombined()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Level,5:w3}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("  inf", sw.ToString());
        }

        private enum Size
        {
            Large
        }

        private class SizeFormatter : IFormatProvider, ICustomFormatter
        {
            private readonly IFormatProvider _innerFormatProvider;

            public SizeFormatter(IFormatProvider innerFormatProvider)
            {
                _innerFormatProvider = innerFormatProvider;
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : _innerFormatProvider.GetFormat(formatType);
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg is Size size)
                {
                    return size == Size.Large ? "Huge" : size.ToString();
                }

                if (arg is IFormattable formattable)
                {
                    return formattable.ToString(format, _innerFormatProvider);
                }

                return arg.ToString();
            }
        }

        [TestMethod]
        public void AppliesCustomFormatterToEnums()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message}", new SizeFormatter(CultureInfo.InvariantCulture));
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Size {Size}", Size.Large));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("Size Huge", sw.ToString());
        }

        [TestMethod]
        public void NonMessagePropertiesAreRendered()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Properties}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.ForContext("Foo", 42).Information("Hello from {Bar}!", "bar"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("{Foo=42}", sw.ToString());
        }

        [TestMethod]
        public void NonMessagePositionalPropertiesAreRendered()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Properties}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.ForContext("Foo", 42).Information("Hello from {0}!", "bar"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("{Foo=42}", sw.ToString());
        }

        [TestMethod]
        public void DoNotDuplicatePropertiesAlreadyRenderedInOutputTemplate()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Foo} {Properties}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.ForContext("Foo", 42).ForContext("Bar", 42).Information("Hello from bar!"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual("42 {Bar=42}", sw.ToString());
        }

        [TestMethod]
        [DataRow("", "Hello, \"World\"!")]
        [DataRow(":j", "Hello, \"World\"!")]
        [DataRow(":l", "Hello, World!")]
        [DataRow(":lj", "Hello, World!")]
        [DataRow(":jl", "Hello, World!")]
        public void AppliesLiteralFormattingToMessageStringsWhenSpecified(string format, string expected)
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message" + format + "}", null);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello, {Name}!", "World"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual(expected, sw.ToString());
        }

        [TestMethod]
        [DataRow("", "{Name=\"World\"}")]
        [DataRow(":j", "{\"Name\": \"World\"}")]
        [DataRow(":lj", "{\"Name\": \"World\"}")]
        [DataRow(":jl", "{\"Name\": \"World\"}")]
        public void AppliesJsonFormattingToMessageStructuresWhenSpecified(string format, string expected)
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message" + format + "}", null);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("{@Obj}", new { Name = "World" }));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual(expected, sw.ToString());
        }

        [TestMethod]
        [DataRow("", "{Name=\"World\"}")]
        [DataRow(":j", "{\"Name\": \"World\"}")]
        [DataRow(":lj", "{\"Name\": \"World\"}")]
        [DataRow(":jl", "{\"Name\": \"World\"}")]
        public void AppliesJsonFormattingToPropertiesTokenWhenSpecified(string format, string expected)
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Properties" + format + "}", null);
            var evt = DelegatingSink.GetLogEvent(l => l.ForContext("Name", "World").Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.AreEqual(expected, sw.ToString());
        }

        [TestMethod]
        public void AnEmptyPropertiesTokenIsAnEmptyStructureValue()
        {
            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Properties}", null);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            // /!\ different behavior from Serilog Core : https://github.com/serilog/serilog/blob/5c3a7821aa0f654e551dc21e8e19089f6767666b/test/Serilog.Tests/Formatting/Display/MessageTemplateTextFormatterTests.cs#L268-L278
            //
            // var expected = new StructureValue(Enumerable.Empty<LogEventProperty>()).ToString();
            // // expected == "{  }"
            // Assert.AreEqual(expected, sw.ToString());
            //
            Assert.AreEqual("{}", sw.ToString());
        }

        [TestMethod]
        [DataRow("", true)]
        [DataRow(":lj", true)]
        [DataRow(":jl", true)]
        [DataRow(":j", false)]
        [DataRow(":l", true)]
        public void FormatProviderWithScalarProperties(string format, bool shouldUseCustomFormatter)
        {
            var frenchFormatProvider = new CultureInfo("fr-FR");
            var defaultFormatProvider = CultureInfo.InvariantCulture;

            var date = new DateTime(2018, 01, 01);
            const double number = 12.345;

            var expectedFormattedDate = shouldUseCustomFormatter
                ? date.ToString(frenchFormatProvider)
                : date.ToString("O", defaultFormatProvider);

            var expectedFormattedNumber = shouldUseCustomFormatter
                ? number.ToString(frenchFormatProvider)
                : number.ToString(defaultFormatProvider);

            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message" + format + "}", frenchFormatProvider);

            var evt = DelegatingSink.GetLogEvent(l =>
            {
                l.Information("{MyDate}{MyNumber}", date, number);
            });

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.Contains(expectedFormattedDate, sw.ToString());
            Assert.Contains(expectedFormattedNumber, sw.ToString());
        }

        [TestMethod]
        [DataRow("", true)]
        [DataRow(":lj", false)]
        [DataRow(":jl", false)]
        [DataRow(":j", false)]
        [DataRow(":l", true)]
        public void FormatProviderWithDestructuredProperties(string format, bool shouldUseCustomFormatter)
        {
            var frenchFormatProvider = new CultureInfo("fr-FR");
            var defaultFormatProvider = CultureInfo.InvariantCulture;

            var date = new DateTime(2018, 01, 01);
            const double number = 12.345;

            var expectedFormattedDate = shouldUseCustomFormatter
                ? date.ToString(frenchFormatProvider)
                : date.ToString("O", defaultFormatProvider);
            var expectedFormattedNumber = shouldUseCustomFormatter
                ? number.ToString(frenchFormatProvider)
                : number.ToString(defaultFormatProvider);

            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Message" + format + "}", frenchFormatProvider);
            var evt = DelegatingSink.GetLogEvent(l =>
            {
                l.Information("{@Item}", new
                {
                    MyDate = date,
                    MyNumber = number,
                });
            });

            var sw = new RichTextLineWriter();
            formatter.Format(evt, sw);

            Assert.Contains(expectedFormattedDate, sw.ToString());
            Assert.Contains(expectedFormattedNumber, sw.ToString());
        }

        [TestMethod]
        public void ExceptionTextDoesNotEndWithANewline()
        {
            Exception exception;
            try
            {
                throw new InvalidOperationException("boom");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var formatter = new RichTextOutputTemplateRenderer(RichTextBoxTheme.None, "{Exception}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Error(exception, "failed"));
            var writer = new RichTextLineWriter();
            formatter.Format(evt, writer);

            var text = writer.ToString();
            Assert.Contains("boom", text);
            Assert.Contains(Environment.NewLine, text);
            Assert.IsFalse(text.EndsWith("\n") || text.EndsWith("\r"));
        }

        [TestMethod]
        public void CustomThemeStyleIsApplied()
        {
            var formatter = new RichTextOutputTemplateRenderer(new RedTextTheme(), "{Message}", CultureInfo.InvariantCulture);
            var evt = DelegatingSink.GetLogEvent(l => l.Information("Hello"));
            var writer = new RichTextLineWriter();
            formatter.Format(evt, writer);

            var line = writer.ToLine();
            Assert.IsTrue(line.Fragments.Exists(fragment => fragment.Text == "Hello" && fragment.Style.Foreground == Brushes.Red));
        }

        private sealed class RedTextTheme : RichTextBoxTheme
        {
            public override RichTextRunStyle GetStyle(RichTextBoxThemeStyle style)
            {
                if (style == RichTextBoxThemeStyle.Text)
                {
                    return new RichTextRunStyle { Foreground = Brushes.Red };
                }

                return default;
            }
        }
    }
}
