using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FluxHelper
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern int SetForegroundWindow(IntPtr point);
    }
    class Program
    {
        static void toggle()
        {
            Process flux = Process.GetProcessesByName("explorer").FirstOrDefault();
            if (flux != null)
            {
                IntPtr h = flux.MainWindowHandle;
                NativeMethods.SetForegroundWindow(h);
                SendKeys.SendWait("%{END}");
            }

            Process league = Process.GetProcessesByName("League of Legends").FirstOrDefault();
            if (league != null)
            {
                IntPtr h = league.MainWindowHandle;
                NativeMethods.SetForegroundWindow(h);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            String monitoredProcess = "League of Legends.exe";
            WqlEventQuery queryCreate = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\" And TargetInstance.Name = \"" + monitoredProcess + "\"");
            WqlEventQuery queryDelete = new WqlEventQuery("__InstanceDeletionEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\" And TargetInstance.Name = \"" + monitoredProcess + "\"");

            ManagementEventWatcher watcherCreate = new ManagementEventWatcher();
            watcherCreate.Query = queryCreate;
            watcherCreate.EventArrived += delegate {
                toggle();
            };
            watcherCreate.Start();

            ManagementEventWatcher watcherDelete = new ManagementEventWatcher();
            watcherDelete.Query = queryDelete;
            watcherDelete.EventArrived += delegate {
                toggle();
            };
            watcherDelete.Start();

            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "Flux Helper";
            trayIcon.Icon = new Icon(Properties.Resources.icon, 40, 40);

            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", delegate {
                trayIcon.Dispose();
                watcherCreate.Stop();
                watcherCreate.Dispose();
                watcherDelete.Stop();
                watcherDelete.Dispose();
                Application.Exit();
            });

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            Application.Run();
        }
    }
}
