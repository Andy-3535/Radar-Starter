using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using MahApps.Metro.Controls;
using System.Net;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static string LocalIpAdress;
        public static string AllIpAdress;
        public static string SomeText;

        public MainWindow()
        {
            InitializeComponent();
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
                MessageBox.Show("Java Error " + ex.Message);
                Process.Start("http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html");
                Environment.Exit(0);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox1.IsChecked == true)
            {
                Radar_Launcher.Properties.Settings.Default.CheckBoxSave = true;
                SomeText = TextBox1.Text;
                Radar_Launcher.Properties.Settings.Default.TextBoxSave = SomeText;
                Radar_Launcher.Properties.Settings.Default.Save();
                var a = Directory.GetFiles(Environment.CurrentDirectory, "arpspoof.exe");
                if (a.Length == 0)
                {
                    MessageBox.Show("No arpspoof.exe!");
                    Process.Start("https://github.com/alandau/arpspoof/releases");
                    Environment.Exit(0);
                }
                Process.Start("arpspoof.exe", SomeText);
                System.Threading.Thread.Sleep(1000);
                var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
                if (v.Length == 0)
                {
                    MessageBox.Show("No jar radar file!");
                    Process.Start("https://github.com/Lafko/Radar-Starter/releases");
                    Environment.Exit(0);
                }
                foreach (var s in v)
                {
                    string b = s;
                    string p = Path.GetFileName(b);

                    Process.Start("java", " -jar " + p + " " + LocalIpAdress + " PortFilter " + SomeText);
                }
                Environment.Exit(0);
            }
            Radar_Launcher.Properties.Settings.Default.CheckBoxSave = false;
            Radar_Launcher.Properties.Settings.Default.Save();
            var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
            if (f.Length == 0)
            {
                MessageBox.Show("No jar radar file!");
                Process.Start("https://github.com/Lafko/Radar-Starter/releases");
                Environment.Exit(0);
            }
            foreach (var s in f)
            {
                string a = s;
                string p = Path.GetFileName(a);
               
                Process.Start("java", " -jar " + p + " " + LocalIpAdress + " PortFilter " + AllIpAdress);
            }
            Environment.Exit(0);
        }

        private void GetLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
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
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback & ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
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
    }

}
