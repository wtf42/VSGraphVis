using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSGraphViz
{
    [Guid(GuidList.guidToolWindowPersistenceString)]
    public class ToolWindow : ToolWindowPane
    {
        public ToolWindow() :  
            base(null)
        {
            base.Content = VSGraphVizPackage.ToolWindowCtl;
            base.Caption = "VS Graph Vis";
        }
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            VSGraphVizPackage.ToolWindowCtl.TrackSelection = (ITrackSelection)GetService(typeof(STrackSelection));
        }
    }
}
