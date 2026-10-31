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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Documents;
using Serilog.Sinks.RichTextBox.Output;
using Serilog.Sinks.RichTextBox.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Serilog.Sinks.RichTextBox.Wpf.Tests.Output
{
    [TestClass]
    public class ParagraphRichTextBoxOutputAppenderTests
    {
        [TestMethod]
        public void FirstLineReplacesTheEmptyPlaceholder()
        {
            RunSta(() =>
            {
                var (box, document) = CreateDocumentWithPlaceholder();
                var appender = new ParagraphRichTextBoxOutputAppender(new ParagraphRichTextBoxOutputAppenderArgs());

                appender.Append(box, document, Line("hello"));

                Assert.AreEqual("hello", Text(document));
            });
        }

        [TestMethod]
        public void LoggedBlankLineSurvivesTheNextBatch()
        {
            RunSta(() =>
            {
                var (box, document) = CreateDocumentWithPlaceholder();
                var appender = new ParagraphRichTextBoxOutputAppender(new ParagraphRichTextBoxOutputAppenderArgs());

                appender.Append(box, document, Line(Environment.NewLine));
                appender.Append(box, document, Line("next"));

                var paragraphs = new List<Paragraph>();
                foreach (var block in document.Blocks)
                {
                    Assert.IsInstanceOfType<Paragraph>(block);
                    paragraphs.Add((Paragraph)block);
                }

                Assert.AreEqual(2, paragraphs.Count);
                Assert.AreEqual(string.Empty, new TextRange(paragraphs[0].ContentStart, paragraphs[0].ContentEnd).Text);
                Assert.AreEqual("next", new TextRange(paragraphs[1].ContentStart, paragraphs[1].ContentEnd).Text);
            });
        }

        [TestMethod]
        public void TrailingNewlineInsideTheLastRunIsRemoved()
        {
            RunSta(() =>
            {
                var (box, document) = CreateDocumentWithPlaceholder();
                var appender = new ParagraphRichTextBoxOutputAppender(new ParagraphRichTextBoxOutputAppenderArgs());

                appender.Append(box, document, Line("boom" + Environment.NewLine));

                Assert.AreEqual("boom", Text(document));
            });
        }

        private static (System.Windows.Controls.RichTextBox Box, FlowDocument Document) CreateDocumentWithPlaceholder()
        {
            var box = new System.Windows.Controls.RichTextBox();
            var document = box.Document;
            if (document.Blocks.Count == 0)
            {
                document.Blocks.Add(new Paragraph());
            }

            return (box, document);
        }

        private static string Text(FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text.TrimEnd('\r', '\n');
        }

        private static IReadOnlyList<RichTextLine> Line(string text)
        {
            return new[]
            {
                new RichTextLine(new List<RichTextFragment> { new(text, default) }),
            };
        }

        private static void RunSta(Action action)
        {
            Exception? caught = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (caught != null)
            {
                ExceptionDispatchInfo.Capture(caught).Throw();
            }
        }
    }
}
