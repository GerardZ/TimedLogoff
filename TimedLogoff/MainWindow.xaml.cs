using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TimedLogoff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int timerGrantStart = 300;
        
        private int timerGrant = timerGrantStart;

        private DispatcherTimer timer;


        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            mainLabel.Content = $"U wordt over {timerGrant--} seconden uitgelogd...";

            progress.Value = timerGrant * 100/timerGrantStart;

            if(timerGrant < 1)
            {
                timer.Stop();
                WindowsLogOff(true);
                this.Close();
            }
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
            if (force) ExitWindowsEx(0 | 0x00000004, 0);
            return ExitWindowsEx(0, 0);
        }
    }
}
