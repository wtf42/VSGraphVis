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
        public Control control;
        public ToolWindow() :  
            base(null)
        {
            base.Caption = "VS Graph Vis";

            control = new Control();
            base.Content = control;
        }
        public WindowStatus windowFrameEventsHandler;
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            control.TrackSelection = (ITrackSelection)GetService(typeof(STrackSelection));
            windowFrameEventsHandler = new WindowStatus(this);
            (this.Frame as IVsWindowFrame).SetProperty(
                (int)__VSFPROPID.VSFPROPID_ViewHelper, windowFrameEventsHandler);
        }
    }
    public sealed class WindowStatus : IVsWindowFrameNotify3
    {
        ToolWindow window;
        public WindowStatus(ToolWindow window)
        {
            this.window = window;
        }
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
            window.control.OnSizeHandler();
            return VSConstants.S_OK;
        }
    }
}
