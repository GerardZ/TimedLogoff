using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

using System.Management;

namespace TimedLogoff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int timerGrantStart = 300;

        private int timerGrant;

        private DispatcherTimer timer;

        private EndMode endMode = EndMode.Sleep;



        public MainWindow()
        {
            InitializeComponent();
            HandleCommandLineOptions();
            timerGrant = timerGrantStart;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void HandleCommandLineOptions()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            Console.WriteLine("GetCommandLineArgs: {0}", string.Join(", ", arguments));

            foreach (string arg in arguments)
            {
                if (arg.ToLower() == "sleep") endMode = EndMode.Sleep;
                if (arg.ToLower() == "shutdown") endMode = EndMode.Shutdown;
                if (arg.ToLower() == "logoff") endMode = EndMode.Logoff;

                int timerGrantStartValue;

                if (Int32.TryParse(arg, out timerGrantStartValue)) timerGrantStart = timerGrantStartValue;

                if (true) { }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (timerGrant < 0)
            {
                timer.Stop();
                if (endMode == EndMode.Shutdown) WindowsShutdown(true);
                if (endMode == EndMode.Sleep) WindowsSleep();
                if (endMode == EndMode.Logoff) WindowsLogOff(true);
                this.Close();
            }

            if (endMode == EndMode.Shutdown) mainLabel.Content = $"Pc gaat over {timerGrant--} seconden shutdown...";
            if (endMode == EndMode.Sleep) mainLabel.Content = $"Pc gaat over {timerGrant--} seconden in sleep mode...";
            if (endMode == EndMode.Logoff) mainLabel.Content = $"U wordt over {timerGrant--} seconden uitgelogd...";

            progress.Value = timerGrant * 100 / timerGrantStart;
            
            try  // when locked this may raise an exception
            {
                Application.Current.MainWindow.Activate();
            }
            catch { }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        


        // WindowsLogoff
        // https://stackoverflow.com/questions/14466373/log-off-a-windows-user-locally-using-c-sharp
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        public static bool WindowsLogOff(bool force = false)
        {
            if (force) return ExitWindowsEx(0 | 0x00000004, 0);
            return ExitWindowsEx(0, 0);
        }

        // Sleep
        // https://stackoverflow.com/questions/2079813/c-sharp-put-pc-to-sleep-or-hibernate
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        public static void WindowsSleep()
        {
            SetSuspendState(true, true, true);
        }


        // Shutdown
        // https://stackoverflow.com/questions/102567/how-to-shut-down-the-computer-from-c-sharp
        // force: https://social.msdn.microsoft.com/Forums/vstudio/en-US/93e33fe7-94aa-4161-aaf5-ddd0a7217b29/how-to-force-a-shutdown-of-window-even-if-connected-client?forum=wpf
        public static void WindowsShutdown(bool force)
        {
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system
            mboShutdownParams["Flags"] = "1";       // shutdown
            mboShutdownParams["Flags"] = "5";       // shutdown forced
            mboShutdownParams["Reserved"] = "0";

            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
        }

        // Settings
        // https://www.c-sharpcorner.com/UploadFile/f9f215/windows-registry/

        private void SaveSettings(Settings settings)
        {

        }
    }
    public class Settings
    {
        public string mode { get; set; }
    }

    enum EndMode
    {
        Logoff,
        Sleep,
        Shutdown
    }
}
