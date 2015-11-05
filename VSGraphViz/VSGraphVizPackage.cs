using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using EnvDTE90;

namespace VSGraphViz
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("VSGraphViz", "Debugger extension displaying graphs", "0.1")]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ToolWindow),
        MultiInstances = false,
        Width = 200, Height = 200,
        Orientation = ToolWindowOrientation.Right)]
    [Guid(GuidList.guidCommandTargetRGBPkgString)]
    public sealed class VSGraphVizPackage : Package
    {
        public VSGraphVizPackage()
        {
        }
        
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // For a multi-instance ToolWindow, find an unused ID
            int id = FindUnusedToolWindowId(typeof(ToolWindow));

            // Create the window with the unused ID.
            var window = CreateToolWindow(typeof(ToolWindow), id) as ToolWindow;
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException();
            }

            // Display the window.
            IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
        
        private int FindUnusedToolWindowId(Type toolWindowType)
        {
            for (int id = 0; ; ++id)
            {
                ToolWindowPane window = FindToolWindow(toolWindowType, id, false);
                if (window == null)
                {
                    return id;
                }
            }
        }

        private void _debuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            if (viz.root_expression == null)
                return;
            var expr = applicationObject.Debugger.GetExpression(viz.root_expression.Name);
            viz.UpdateGraph(expr);
        }


        public static Control ToolWindowCtl;
        public static VSGraphVisualizer viz;
        DTE2 applicationObject;
        DebuggerEvents debuggerEvents;

        protected override void Initialize()
        {
            base.Initialize();

            applicationObject = (DTE2)GetService(typeof(DTE));
            debuggerEvents = applicationObject.Events.DebuggerEvents;
            debuggerEvents.OnEnterBreakMode += _debuggerEvents_OnEnterBreakMode;

            ToolWindowCtl = new Control();
            viz = new VSGraphVisualizer();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidCommandTargetRGBCmdSet, (int)PkgCmdIDList.cmdidShowToolWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }
        }

        public static void VSOutputLog(string str)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
            string customTitle = "VSGraphViz";
            outWindow.CreatePane(ref customGuid, customTitle, 1, 1);

            IVsOutputWindowPane customPane;
            outWindow.GetPane(ref customGuid, out customPane);

            customPane.OutputString(str + "\n");
            customPane.Activate(); // Brings this pane into view
        }
    }
}
