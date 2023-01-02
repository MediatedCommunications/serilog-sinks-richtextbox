using System.Collections.Generic;
using System.Windows.Documents;
using System;
using System.Windows.Markup;

namespace Serilog.Sinks.RichTextBox.Output
{
    public abstract class RichTextBoxOutputAppenderBase<TArgs> : RichTextBoxOutputAppenderBase
        where TArgs : RichTextBoxOutputAppenderArgs
    {

        protected TArgs Args { get; }

        public RichTextBoxOutputAppenderBase(TArgs args)
        {
            this.Args = args;
        }

    }

}
