using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Interop;
using System.Windows.Forms;

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
        public WindowStatus windowFrameEventsHandler;
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            VSGraphVizPackage.ToolWindowCtl.TrackSelection = (ITrackSelection)GetService(typeof(STrackSelection));
            windowFrameEventsHandler = new WindowStatus();
            (this.Frame as IVsWindowFrame).SetProperty(
                (int)__VSFPROPID.VSFPROPID_ViewHelper, windowFrameEventsHandler);
        }
    }
    public sealed class WindowStatus : IVsWindowFrameNotify3
    {
        public WindowStatus()
        { }
        public int OnClose(ref uint pgrfSaveOptions)
        {
            return VSConstants.S_OK;
        }
        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            return VSConstants.S_OK;
        }
        public int OnMove(int x, int y, int w, int h)
        {
            return VSConstants.S_OK;
        }
        public int OnShow(int fShow)
        {
            return VSConstants.S_OK;
        }
        public int OnSize(int x, int y, int w, int h)
        {
            //VSGraphVizPackage.VSOutputLog(w.ToString() +":"+ h.ToString());
            VSGraphVizPackage.ToolWindowCtl.OnSizeHandler();
            return VSConstants.S_OK;
        }
    }
}
