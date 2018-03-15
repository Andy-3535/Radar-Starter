using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string LocalIpAdress;
        public static string AllIpAdress;

        public MainWindow()
        {
            InitializeComponent();
            findJavaVersion();
            GetLocalIp();
            GetAllLocalIp();
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
            var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar");
            if (f.Length == 0)
            {
                MessageBox.Show("No jar radar file!");
                Process.Start("https://github.com/ReddeR1337/PUBG-Radar/releases");
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
                    Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            comboBoxLanInternet.Items.Add(ip.Address.ToString());
                        }
                    }
                }
            }
        }

        private void GetAllLocalIp()
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        comboBoxLanInternet2.Items.Add(ip.Address.ToString());
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
    }

}
