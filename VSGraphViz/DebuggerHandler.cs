using EnvDTE;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace VSGraphViz
{
    public sealed class DebuggerHandler
    {
        public const string AdornmentLayerName = "DebuggerHandler";

        private EnvDTE.Debugger m_debugger;
        private IWpfTextView m_view;

        public DebuggerHandler(EnvDTE.Debugger debugger, IWpfTextView view)
        {
            m_debugger = debugger;
            m_view = view;
            m_view.MouseHover += OnMouseHover;
        }

        private void OnMouseHover(object sender, MouseHoverEventArgs e)
        {
            if (m_debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return;
            }

            var exp = FindUnderMousePointer(m_debugger, e);
            if (exp == null)
                return;
            if (!exp.IsValidValue)
                return;
            VSGraphVizPackage.viz.UpdateGraph(exp);
        }

        Expression FindUnderMousePointer(EnvDTE.Debugger debugger, MouseHoverEventArgs e)
        {
            var point = e.TextPosition.GetPoint(e.TextPosition.AnchorBuffer, PositionAffinity.Predecessor);
            if (!point.HasValue)
            {
                return null;
            }

            SnapshotSpan span;
            var name = GetVariableNameAndSpan(point.Value, out span);
            if (name == null)
            {
                return null;
            }

            var expression = debugger.GetExpression(name);
            if (!expression.IsValidValue)
            {
                return null;
            }
            return expression;
        }

        private static Regex m_variableExtractor = new Regex("[a-zA-Z0-9_.]+");
        private static string GetVariableNameAndSpan(SnapshotPoint point, out SnapshotSpan span)
        {
            var line = point.GetContainingLine();
            var hoveredIndex = point.Position - line.Start.Position;

            var c = point.GetChar();
            if (!Char.IsDigit(c) && !Char.IsLetter(c) && (c != '_'))
            {
                span = new SnapshotSpan();
                return null;
            }

            // Find the name of the variable under the mouse pointer (ex: 'gesture.Pose.Name' when the mouse is hovering over the 'o' of pose)
            var match = m_variableExtractor.Matches(line.GetText()).OfType<Match>().SingleOrDefault(x => x.Index <= hoveredIndex && (x.Index + x.Length) >= hoveredIndex);
            if ((match == null) || (match.Value.Length == 0))
            {
                span = new SnapshotSpan();
                return null;
            }
            var name = match.Value;

            // Find the first '.' after the hoveredIndex and cut it off
            int relativeIndex = hoveredIndex - match.Index;
            var lastIndex = name.IndexOf('.', relativeIndex);
            if (lastIndex >= 0)
            {
                name = name.Substring(0, lastIndex);
            }
            else
            {
                lastIndex = name.Length;
            }

            var matchStartIndex = name.LastIndexOf('.', relativeIndex) + 1;
            span = new SnapshotSpan(line.Start.Add(match.Index + matchStartIndex), lastIndex - matchStartIndex);

            return name;
        }
    }
}
