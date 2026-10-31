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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using Serilog.Sinks.RichTextBox.Output;
using Serilog.Sinks.RichTextBox.Rendering;

namespace Serilog.Sinks.RichTextBox.Abstraction
{
    internal class RichTextBoxImpl : IRichTextBox
    {
        private readonly DispatcherPriority _priority;
        private readonly FlowDocument[] _richTextBoxes;
        private readonly ILookup<Dispatcher, FlowDocument> _richTextBoxesByDispatcher;
        private readonly IRichTextBoxOutputAppender _outputAppender;

        public RichTextBoxImpl(DispatcherPriority priority, IRichTextBoxOutputAppender outputAppender, params FlowDocument[] documents)
        {
            _priority = priority;
            _richTextBoxes = documents?.ToArray() ?? throw new ArgumentNullException(nameof(documents));
            _outputAppender = outputAppender ?? throw new ArgumentNullException(nameof(outputAppender));

            _richTextBoxesByDispatcher = _richTextBoxes.ToLookup(x => x.Dispatcher);
        }

        public async Task WriteAsync(IReadOnlyList<RichTextLine> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            foreach (var pair in _richTextBoxesByDispatcher)
            {
                var documents = pair.ToArray();
                await pair.Key.InvokeAsync(() => Write(lines, documents), _priority).Task
                    .ConfigureAwait(false);
            }
        }

        private void Write(IReadOnlyList<RichTextLine> lines, IEnumerable<FlowDocument> documents)
        {
            foreach (var document in documents)
            {
                var richTextBox = document.FindLogicalAncestor<System.Windows.Controls.RichTextBox>();
                if (richTextBox is { IsUndoEnabled: true })
                {
                    richTextBox.IsUndoEnabled = false;
                }

                _outputAppender.Append(richTextBox, document, lines);
            }
        }
    }
}
