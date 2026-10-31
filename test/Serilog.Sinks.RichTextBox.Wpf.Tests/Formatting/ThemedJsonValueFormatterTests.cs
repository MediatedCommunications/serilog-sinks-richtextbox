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
using Serilog.Sinks.RichTextBox.Formatting;
using Serilog.Sinks.RichTextBox.Rendering;
using Serilog.Sinks.RichTextBox.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Serilog.Sinks.RichTextBox.Wpf.Tests.Formatting
{
    [TestClass]
    public class ThemedJsonValueFormatterTests
    {
        private class TestThemedJsonValueFormatter : ThemedJsonValueFormatter
        {
            public TestThemedJsonValueFormatter()
                : base(RichTextBoxTheme.None, null)
            {
            }

            public string Format(object literal)
            {
                var output = new RichTextLineWriter();
                Format(new SequenceValue(new[] { new ScalarValue(literal) }), output, null);

                var o = output.ToString();

                return o.Substring(1, o.Length - 2);
            }
        }

        [TestMethod]
        [DataRow(123, "123")]
        [DataRow('c', "\"c\"")]
        [DataRow("Hello, world!", "\"Hello, world!\"")]
        [DataRow(true, "true")]
        [DataRow("\\\"\t\r\n\f", "\"\\\\\\\"\\t\\r\\n\\f\"")]
        [DataRow("\u0001", "\"\\u0001\"")]
        [DataRow("a\nb", "\"a\\nb\"")]
        [DataRow(null, "null")]
        [DataRow("Hello, <>&\"' world!", "\"Hello, <>&\\\"' world!\"")]
        public void JsonLiteralTypesAreFormatted(object value, string expectedJson)
        {
            var formatter = new TestThemedJsonValueFormatter();
            Assert.AreEqual(expectedJson, formatter.Format(value));
        }

        [TestMethod]
        public void DateTimesFormatAsIso8601()
        {
            JsonLiteralTypesAreFormatted(new DateTime(2016, 01, 01, 13, 13, 13, DateTimeKind.Utc),
                "\"2016-01-01T13:13:13.0000000Z\"");
        }

        [TestMethod]
        public void DoubleFormatsAsNumber()
        {
            JsonLiteralTypesAreFormatted(123.45, "123.45");
        }

        [TestMethod]
        public void DoubleSpecialsFormatAsString()
        {
            JsonLiteralTypesAreFormatted(double.NaN, "\"NaN\"");
            JsonLiteralTypesAreFormatted(double.PositiveInfinity, "\"Infinity\"");
            JsonLiteralTypesAreFormatted(double.NegativeInfinity, "\"-Infinity\"");
        }

        [TestMethod]
        public void FloatFormatsAsNumber()
        {
            JsonLiteralTypesAreFormatted(123.45f, "123.45");
        }

        [TestMethod]
        public void FloatSpecialsFormatAsString()
        {
            JsonLiteralTypesAreFormatted(float.NaN, "\"NaN\"");
            JsonLiteralTypesAreFormatted(float.PositiveInfinity, "\"Infinity\"");
            JsonLiteralTypesAreFormatted(float.NegativeInfinity, "\"-Infinity\"");
        }

        [TestMethod]
        public void DecimalFormatsAsNumber()
        {
            JsonLiteralTypesAreFormatted(123.45m, "123.45");
        }

        private static string Format(LogEventPropertyValue value)
        {
            var formatter = new TestThemedJsonValueFormatter();
            var output = new RichTextLineWriter();

            formatter.Format(value, output, null);

            return output.ToString();
        }

        [TestMethod]
        public void ScalarPropertiesFormatAsLiteralValues()
        {
            var f = Format(new ScalarValue(123));
            Assert.AreEqual("123", f);
        }

        [TestMethod]
        public void SequencePropertiesFormatAsArrayValue()
        {
            var f = Format(new SequenceValue(new[] { new ScalarValue(123), new ScalarValue(456) }));
            Assert.AreEqual("[123, 456]", f);
        }

        [TestMethod]
        public void StructuresFormatAsAnObject()
        {
            var structure = new StructureValue(new[] { new LogEventProperty("A", new ScalarValue(123)) }, "T");
            var f = Format(structure);
            Assert.AreEqual("{\"A\": 123, \"$type\": \"T\"}", f);
        }

        [TestMethod]
        public void DictionaryWithScalarKeyFormatsAsAnObject()
        {
            var dict = new DictionaryValue(new Dictionary<ScalarValue, LogEventPropertyValue>
            {
                { new ScalarValue(12), new ScalarValue(345) },
            });

            var f = Format(dict);
            Assert.AreEqual("{\"12\": 345}", f);
        }

        [TestMethod]
        public void SequencesOfSequencesAreFormatted()
        {
            var s = new SequenceValue(new[] { new SequenceValue(new[] { new ScalarValue("Hello") }) });

            var f = Format(s);
            Assert.AreEqual("[[\"Hello\"]]", f);
        }
    }
}
