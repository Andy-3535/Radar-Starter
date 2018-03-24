using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.NetworkInformation;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Navigation;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private NotifyIcon MyNotifyIcon;
        public static string LocalIpAdress;
        public static string AllIpAdress;
        public static string SomeText;
        

        public MainWindow()
        {
            InitializeComponent();
            MyNotifyIcon = new NotifyIcon();
            MyNotifyIcon.Icon = new Icon(Radar_Launcher.Properties.Resources.Launcher_icon, 40, 40);
            MyNotifyIcon.MouseDoubleClick += new MouseEventHandler(MyNotifyIcon_MouseDoubleClick);
            findJavaVersion();
            GetLocalIp();
            GetAllLocalIp();
            CheckBox1.IsChecked = Radar_Launcher.Properties.Settings.Default.CheckBoxSave;
            TextBox1.Text = Radar_Launcher.Properties.Settings.Default.TextBoxSave;
        }

        static private void findJavaVersion()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = " -version";
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                Process pr = Process.Start(psi);
                string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Java Error " + ex.Message);
                Process.Start("http://www.oracle.com/technetwork/java/javase/downloads/jdk9-downloads-3848520.html");
                Environment.Exit(0);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.OutputDataReceived += proc_OutputDataReceived;
            if (CheckBox2.IsChecked == true)
            {
                TextBoxCmd.Text = "Launching the radar... \nThis is Offline mod.";
                Radar_Launcher.Properties.Settings.Default.CheckBoxSave = false;
                Radar_Launcher.Properties.Settings.Default.Save();
                var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
                if (f.Length == 0)
                {
                    System.Windows.MessageBox.Show("No jar radar file!");
                    Process.Start("https://github.com/Lafko/Radar-Starter/releases/");

                }
                foreach (var s in f)
                {
                    string a = s;
                    string p = Path.GetFileName(a);
                    string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    string AllIpAdressFilter = Regex.Replace(AllIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + p + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter + " Offline";
                    proc.Start();
                    proc.BeginOutputReadLine();
                }
                
            }
            else
            {
                if (CheckBox1.IsChecked == true)
             {
                    TextBoxCmd.Text = "Launching the radar with arpspoof...";
                 Radar_Launcher.Properties.Settings.Default.CheckBoxSave = true;
                 SomeText = TextBox1.Text;
                 Radar_Launcher.Properties.Settings.Default.TextBoxSave = SomeText;
                 Radar_Launcher.Properties.Settings.Default.Save();
                 var a = Directory.GetFiles(Environment.CurrentDirectory, "arpspoof.exe");
                 if (a.Length == 0)
                 {
                     System.Windows.MessageBox.Show("No arpspoof.exe!");
                     Process.Start("https://github.com/alandau/arpspoof/releases");
                     Environment.Exit(0);
                 }
                 Process.Start("arpspoof.exe", SomeText);
                 System.Threading.Thread.Sleep(1000);
                 var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
                 if (v.Length == 0)
                 {
                     System.Windows.MessageBox.Show("No jar radar file!");
                     Process.Start("https://github.com/Lafko/Radar-Starter/releases/");
                     Environment.Exit(0);
                 }
                 foreach (var s in v)
                 {
                     string b = s;
                     string p = Path.GetFileName(b);
                     string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + p + " " + LocalIpAdressFilter + " PortFilter " + SomeText;
                    proc.Start();
                    proc.BeginOutputReadLine();
                 }
                 
             }
            else
            {
                TextBoxCmd.Text = "Launching the radar... \nThis is Online mod.";
                Radar_Launcher.Properties.Settings.Default.CheckBoxSave = false;
                Radar_Launcher.Properties.Settings.Default.Save();
                    var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
                if (f.Length == 0)
                {
                    System.Windows.MessageBox.Show("No jar radar file!");
                    Process.Start("https://github.com/Lafko/Radar-Starter/releases/");

                }
                    foreach (var s in f)
                {
                    string a = s;
                    string p = Path.GetFileName(a);
                    string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    string AllIpAdressFilter = Regex.Replace(AllIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + p + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter;
                    proc.Start();
                    proc.BeginOutputReadLine();
                }
            }
            }
        }

        private void GetLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            comboBoxLanInternet.Items.Add(ip.Address.ToString() + ": " + ni.Name);
                        }
                    }
                }
            }
        }

        private void GetAllLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            comboBoxLanInternet2.Items.Add(ip.Address.ToString() + ": " + ni.Name);
                        }
                    }
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocalIpAdress = (string)comboBoxLanInternet.SelectedItem;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            AllIpAdress = (string)comboBoxLanInternet2.SelectedItem;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBox1.IsChecked == true)
            {
                CheckBox2.IsChecked = false;
                using (FileStream fs = File.Create(Environment.CurrentDirectory + "/arpspoof.exe"))
                {
                    Byte[] info = Radar_Launcher.Properties.Resources.arpspoof;
                    fs.Write(info, 0, info.Length);
                }
            }
            TextBox1.Visibility = Visibility.Visible;
            Lable1.Visibility = Visibility.Visible;
            comboBoxLanInternet2.Visibility = Visibility.Hidden;
            LabelLanInternet2.Visibility = Visibility.Hidden;
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBox1.Visibility = Visibility.Hidden;
            Lable1.Visibility = Visibility.Hidden;
            comboBoxLanInternet2.Visibility = Visibility.Visible;
            LabelLanInternet2.Visibility = Visibility.Visible;
        }

        private void CheckBox2_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBox2.IsChecked == true)
            {
                CheckBox1.IsChecked = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                TextBoxCmd.Text = TextBoxCmd.Text + "\n" + e.Data;
                TextBoxCmd.ScrollToEnd();
            }));
        }

        void MyNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app VMRadar Launcher";
                MyNotifyIcon.ShowBalloonTip(400);
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {

        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
