using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Serilog.Sinks.RichTextBox.Output
{
    internal static class WpfExtensions
    {
        public static T FindLogicalAncestor<T>(this DependencyObject start) where T : DependencyObject
        {
            DependencyObject current = start;
            while (current != null)
            {
                if (current is T match)
                    return match;
                current = LogicalTreeHelper.GetParent(current);
            }
            return null;
        }

    }
}
