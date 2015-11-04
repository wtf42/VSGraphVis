using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VSGraphViz
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class DebuggerHandlerFactory : IWpfTextViewCreationListener
    {
        private DTE2 m_dte;

        [Export(typeof(AdornmentLayerDefinition))]
        [Name(DebuggerHandler.AdornmentLayerName)]
        [Order(After = PredefinedAdornmentLayers.Text)]
        internal AdornmentLayerDefinition editorAdornmentLayer = null;

        DebuggerHandlerFactory()
        {
            m_dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        }

        public void TextViewCreated(IWpfTextView view)
        {
            view.Properties.GetOrCreateSingletonProperty<DebuggerHandler>(() => new DebuggerHandler(m_dte.Debugger, view));
        }
    }
}
