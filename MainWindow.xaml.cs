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
using System.Net;
using MahApps.Metro;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Win32;
using ApplicationUpdate;
using AutoHotkey.Interop;

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
        public static string SomeText2;
        public static string SomeText3;
        ViewModel viewModel = new ViewModel();
        public static string TextVer;

        public MainWindow()
        {
            InitializeComponent();
            CompareLauncherVersions();
            TextBoxCmd.Text += "------------------------------- Launcher Made by Lafko from https://lafkomods.ru/ -------------------------------";
            ChangeAppStyle();
            DataContext = viewModel;
            TextBoxRadarPCIP.Text = Radar_Launcher.Properties.Settings.Default.TextBoxRadarPCIP;
            TextBoxGamePCIP.Text = Radar_Launcher.Properties.Settings.Default.TextBoxGamePCIP;
            comboBoxTheme.SelectedItem = Radar_Launcher.Properties.Settings.Default.Color;
            FindJavaVersion();
            FindWinPcap();
            MyNotifyIcon = new NotifyIcon();
            MyNotifyIcon.Icon = new Icon(Radar_Launcher.Properties.Resources.Launcher_icon, 40, 40);
            MyNotifyIcon.MouseDoubleClick += new MouseEventHandler(MyNotifyIcon_MouseDoubleClick);
            GetLocalIp();
            GetAllLocalIp();
            RadarCheck();
        }

        private void RadarCheck()
        {
            var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
            if (v.Length > 1)
            {
                TextBoxCmd.Text += "\nWARNING YOU HAVE 2 JAR FILES IN FOLDER!!! Delete one!";
            }
            if (v.Length == 0)
            {
                TextBoxCmd.Text += "\nRadar File nod found.";
            }
            foreach (var s in v)
            {
                if (File.Exists(s))
                {
                    TextBoxCmd.Text += "\nRadar file fund in: " + s;
                    ButtonDorU.IsEnabled = false;
                    ButtonDorU.Content = "Update Radar";
                }
            }
            string localVersion = Versions.LocalVersion(Environment.CurrentDirectory + "/radar.version");
            string remoteVersion = Versions.RemoteVersion("http://j25940kk.beget.tech/pubg/RADAR/" + "radar.version");
            RlocVer.Content = localVersion;
            RLastVer.Content = remoteVersion;
            if (localVersion != remoteVersion)
            {
                ButtonDorU.IsEnabled = true;
            }
        }

        public class ViewModel : INotifyPropertyChanged
        {
            private string _badgeValue;
            public string BadgeValue
            {
                get { return _badgeValue; }
                set { _badgeValue = value; NotifyPropertyChanged(); }
            }

            private string _badgeValue2;
            public string BadgeValue2
            {
                get { return _badgeValue2; }
                set { _badgeValue2 = value; NotifyPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FindJavaVersion()
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
                TextBoxCmd.Text += "\nJava found, version: " + strOutput;
            }
            catch (Exception ex)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("\nJava error: " + ex.Message + ", Download?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("http://www.oracle.com/technetwork/java/javase/downloads/jdk9-downloads-3848520.html");
                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(0);
                } 
            }
        }

        private void FindWinPcap()
        {
            if(IsProgramInstalled("WinPcap 4.1.3", false) == true)
            {
                TextBoxCmd.Text += "\nWinPcap 4.1.3 found.";
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("\nWinPcap 4.1.3 not found, Download?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                            wc.DownloadFileAsync(new Uri("https://www.winpcap.org/install/bin/WinPcap_4_1_3.exe"), Environment.CurrentDirectory + "/WinPcap_4_1_3.exe");
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBoxCmd.Text += "Download Error " + ex.Message;
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = Environment.CurrentDirectory + "/WinPcap_4_1_3.exe";
                process.StartInfo.Arguments = "/quiet";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                FindWinPcap();
                if (File.Exists(Environment.CurrentDirectory + "/WinPcap_4_1_3.exe"))
                {
                    File.Delete(Environment.CurrentDirectory + "/WinPcap_4_1_3.exe");
                }
            }
            catch (Exception ex)
            {
                TextBoxCmd.Text += "\nWinPcap install error: " + ex.Message;
            }
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                TextBoxCmd.Text = TextBoxCmd.Text + "\n" + e.Data;
                TextBoxCmd.ScrollToEnd();
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.OutputDataReceived += proc_OutputDataReceived;
            proc.ErrorDataReceived += proc_OutputDataReceived;
            if (RadioCustomIp.IsChecked == true)
            {
                SomeText2 = TextBoxGamePCIP.Text;
                SomeText3 = TextBoxRadarPCIP.Text;
                Radar_Launcher.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
                Radar_Launcher.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
                Radar_Launcher.Properties.Settings.Default.Save();
                var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
                if (v.Length == 0)
                {
                    TextBoxCmd.Text += "\nNo Radar jar file!\nStarting download radar file...";
                    CompareRadarVersions();
                }
                foreach (var s in v)
                {
                    string b = s;
                    string path = "\"" + Path.GetDirectoryName(b) + "\\" + Path.GetFileName(b) + "\"";
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = " -jar " + path + " " + SomeText3 + " PortFilter " + SomeText2 + " 1 1";
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + path + " " + SomeText3 + " PortFilter " + SomeText2 + " 1 1";
                }
            }
            if (RadioPCAP.IsChecked == true)
            {
                var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
                if (f.Length == 0)
                {
                    TextBoxCmd.Text += "\nNo Radar jar file!\nStarting download radar file...";
                    CompareRadarVersions();
                }
                foreach (var s in f)
                {
                    TextBoxCmd.Text += "\nLaunching the radar... \nThis is Offline mod.";
                    string a = s;
                    string path = "\"" + Path.GetDirectoryName(a) + "\\" + Path.GetFileName(a) + "\"";
                    string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    string AllIpAdressFilter = Regex.Replace(AllIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + path + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter + " Offline";
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + path + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter + " Offline";
                }
            }
            if (RadioArp.IsChecked == true)
            {
                SomeText2 = TextBoxGamePCIP.Text;
                SomeText3 = TextBoxRadarPCIP.Text;
                Radar_Launcher.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
                Radar_Launcher.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
                Radar_Launcher.Properties.Settings.Default.Save();
                Process.Start("arpspoof.exe", SomeText2);
                System.Threading.Thread.Sleep(1000);
                var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
                if (v.Length == 0)
                {
                    TextBoxCmd.Text += "\nNo Radar jar file!\nStarting download radar file...";
                    CompareRadarVersions();
                }
                foreach (var s in v)
                {
                    string b = s;
                    string path = "\"" + Path.GetDirectoryName(b) + "\\" + Path.GetFileName(b) + "\"";
                    string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + path + " " + SomeText3 + " PortFilter " + SomeText2 + " 1 1";
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + path + " " + SomeText3 + " PortFilter " + SomeText2 + " 1 1";
                }
            }
            if (RadioAuto.IsChecked == true)
            {
                var f = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
                if (f.Length == 0)
                {
                    TextBoxCmd.Text += "\nNo Radar jar file!\nStarting download radar file...";
                    CompareRadarVersions();
                }
                foreach (var s in f)
                {
                    TextBoxCmd.Text += "\nLaunching the radar... \nThis is Online mod.";
                    string a = s;
                    string path = "\"" + Path.GetDirectoryName(a) + "\\" + Path.GetFileName(a) + "\"";
                    string LocalIpAdressFilter = Regex.Replace(LocalIpAdress, "[^0-9 .]", "");
                    string AllIpAdressFilter = Regex.Replace(AllIpAdress, "[^0-9 .]", "");
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + path + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter + " 1 1";
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + path + " " + LocalIpAdressFilter + " PortFilter " + AllIpAdressFilter + " 1 1";
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
                            viewModel.BadgeValue = Convert.ToString(comboBoxLanInternet.Items.Count);
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
                            viewModel.BadgeValue2 = Convert.ToString(comboBoxLanInternet2.Items.Count);
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

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxCmd.IsReadOnly = true;
        }

        void MyNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TextBoxCmd.Text = "";
        }

        private void RadioAuto_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Visible;
            comboBoxLanInternet2.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Hidden;
            comboBoxLanInternet2.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioPCAP_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Visible;
            comboBoxLanInternet2.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioPCAP_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Hidden;
            comboBoxLanInternet2.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioArp_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Visible;
            TextBoxRadarPCIP.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioArp_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Hidden;
            TextBoxRadarPCIP.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioCustomIp_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Visible;
            TextBoxRadarPCIP.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioCustomIp_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Hidden;
            TextBoxRadarPCIP.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        public void ChangeAppStyle()
        {
            List<string> themes = new List<string>();
            themes.Add("Red");
            themes.Add("Green");
            themes.Add("Blue");
            themes.Add("Purple");
            themes.Add("Orange");
            themes.Add("Lime");
            themes.Add("Emerald");
            themes.Add("Teal");
            themes.Add("Cyan");
            themes.Add("Cobalt");
            themes.Add("Indigo");
            themes.Add("Violet");
            themes.Add("Pink");
            themes.Add("Magenta");
            themes.Add("Crimson");
            themes.Add("Amber");
            themes.Add("Yellow");
            themes.Add("Brown");
            themes.Add("Olive");
            themes.Add("Steel");
            themes.Add("Mauve");
            themes.Add("Taupe");
            themes.Add("Sienna");
 
            foreach (string theme in themes)
            {
                comboBoxTheme.Items.Add(theme);
            }
        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent((string)comboBoxTheme.SelectedItem), ThemeManager.GetAppTheme("BaseLight"));
            Radar_Launcher.Properties.Settings.Default.Color = (string)comboBoxTheme.SelectedItem;
            Radar_Launcher.Properties.Settings.Default.Save();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Radar_Launcher.Properties.Settings.Default.Save();
        }

        private void MetroWindow_StateChanged_1(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app VMRadar Launcher";
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        public static bool IsProgramInstalled(string displayName, bool x86Platform)
        {
            string uninstallKey = string.Empty;

            if (x86Platform)
            {
                uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            else
            {
                uninstallKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {

                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if (sk.GetValue("DisplayName") != null && sk.GetValue("DisplayName").ToString().ToUpper().Equals(displayName.ToUpper()))
                        {
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        public static bool Contains(string inputString, string strToSearch)
        {
            return Regex.IsMatch(inputString, strToSearch);
        }
        //Removed before problem solved
        /*
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
             string PathToFile = Environment.CurrentDirectory + "/settings.json";
             string text = File.ReadAllText(PathToFile);
             for (int i = 1; i <= 112; i++)
             {
                 text = text.Replace("\"" + sup2[i - 1] +  "\": " + Radar_Launcher.SettingsRad.getBetween(text, "\"" + sup2[i - 1] + "\": ", ",") + ",", "\"" + sup2[i - 1] + "\": " + textboxes[i - 1].Text + ",");
                 File.WriteAllText(PathToFile, text);
             }
        }

        List<System.Windows.Controls.TextBox> textboxes = new List<System.Windows.Controls.TextBox>();
        List<String> sup2 = new List<String>();
        
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "/settings.json"))
            {
                var jsonFile = File.ReadAllLines(Environment.CurrentDirectory + "/settings.json");
                List<String> sup = new List<String>();
                for (int i = 1; i <= 112; i++)
                {
                    sup.Add(Radar_Launcher.SettingsRad.getBetween(jsonFile[i], "\": ", ","));
                    sup2.Add(Radar_Launcher.SettingsRad.getBetween(jsonFile[i], "\"", "\": "));
                    System.Windows.Controls.TextBox newTexBox = new System.Windows.Controls.TextBox();
                    textboxes.Add(newTexBox);
                    var title = ListBox1.Items.Add(sup2[i - 1] + ":");
                    ListBox1.Items.Add(newTexBox);
                    newTexBox.Text = sup[i - 1];
                }
            }
        }
        */
        private void TextBoxJson_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "/settings.json"))
            {
                string PathToFile = Environment.CurrentDirectory + "/settings.json";
                string text = File.ReadAllText(PathToFile);
                TextBoxJson.Text = text;
            }
        }

        private void ButtonSaveJson_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + "/settings.json"))
            {
                using (FileStream fs = File.Create(Environment.CurrentDirectory + "/settings.json"))
                {
                    Byte[] info = Radar_Launcher.Properties.Resources.settings;
                    fs.Write(info, 0, info.Length);
                }
                TextBoxJson.Text = File.ReadAllText(Environment.CurrentDirectory + "/settings.json");
            }
            else
            {
                File.WriteAllText(Environment.CurrentDirectory + "/settings.json", TextBoxJson.Text);
            }
        }

        private void BeginDownload(string remoteURL, string downloadToPath, string version, string executeTarget)
        {
            string filePath = Versions.CreateTargetLocation(downloadToPath, version);

            Uri remoteURI = new Uri(remoteURL);
            WebClient downloader = new WebClient();
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wb_DownloadProgressChanged);
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
            downloader.DownloadFileCompleted += wc_DownloadFileCompleted2;
            downloader.DownloadFileAsync(remoteURI, filePath + ".zip", new string[] { version, downloadToPath, executeTarget });
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string[] us = (string[])e.UserState;
            string currentVersion = us[0];
            string downloadToPath = us[1];
            string executeTarget = us[2];

            if (!downloadToPath.EndsWith("\\"))
                
                downloadToPath += "\\";

            // Download folder + zip file
            string zipName = downloadToPath + currentVersion + ".zip";
            // Download folder\version\ + executable
            string exePath = downloadToPath + currentVersion + "\\" + executeTarget;

            if (new FileInfo(zipName).Exists)
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipName))
                {
                    if (TextVer == "app.version")
                    {
                        if (File.Exists(Environment.CurrentDirectory + "/Radar Launcher.bak"))
                        {
                            File.Delete(Environment.CurrentDirectory + "/Radar Launcher.bak");
                        }
                        File.Move(AppDomain.CurrentDomain.FriendlyName, "Radar Launcher.bak");
                        zip.ExtractAll(downloadToPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                        
                        MessageBoxResult result = System.Windows.MessageBox.Show("Launcher updated, restart app!", "MessageBoxResult", MessageBoxButton.OK, MessageBoxImage.Question);
                        if (result == MessageBoxResult.OK)
                        {
                            Process proc = Process.Start(downloadToPath + "\\" + executeTarget);
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        zip.ExtractAll(downloadToPath + currentVersion, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                    if (new FileInfo(exePath).Exists)
                {
                    Versions.CreateLocalVersionFile(downloadToPath, TextVer, currentVersion);
                    Process proc = Process.Start(exePath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Problem with download. File does not exist.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Problem with download. File does not exist.");
            }
        }

        void wb_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void CompareRadarVersions()
        {
            TextVer = "radar.version";
            string downloadToPath = Environment.CurrentDirectory;
            string localVersion = Versions.LocalVersion(downloadToPath + "/radar.version");
            string remoteURL = "http://j25940kk.beget.tech/pubg/RADAR/";
            string remoteVersion = Versions.RemoteVersion(remoteURL + "radar.version");
            string remoteFile = remoteURL + remoteVersion + ".zip";
            if (localVersion != remoteVersion)
            {
                Button1.IsEnabled = false;
                BeginDownload(remoteFile, downloadToPath, remoteVersion, "update.txt");
            }
        }

        private void CompareLauncherVersions()
        {
            TextVer = "app.version";
            string downloadToPath = Environment.CurrentDirectory;
            string localVersion = Versions.LocalVersion(downloadToPath + "/app.version");
            string remoteURL = "http://j25940kk.beget.tech/pubg/launcher/";
            string remoteVersion = Versions.RemoteVersion(remoteURL + "app.version");
            string remoteFile = remoteURL + remoteVersion + ".zip";

            LabelLocalVer.Content = localVersion;
            LabelLastVer.Content = remoteVersion;
            if (localVersion != remoteVersion)
            {
                BeginDownload(remoteFile, downloadToPath, remoteVersion, "update.txt");
            }
        }

        private void wc_DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            if (progressBar1.Value == 100)
            {
                ButtonDorU.IsEnabled = false;
                progressBar1.Value = 0;
                ButtonDorU.Content = "Update Done";
                RadarCheck();
                MessageBoxResult result = System.Windows.MessageBox.Show("Clear the directory from zip files??", "Cleaning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var v = Directory.GetFiles(Environment.CurrentDirectory, "*.zip", SearchOption.AllDirectories);
                    foreach (var s in v)
                    {
                        File.Delete(s);
                    }
                }
                Button1.IsEnabled = true;
            }
        }

        private void ProgBarDownload_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ButtonDorU.IsEnabled = false;
            ButtonDorU.Content = progressBar1.Value + "%...";
        }

        private void ButtonDorU_Click(object sender, RoutedEventArgs e)
        {
            CompareRadarVersions();
        }

        private void CheckBoxAim_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("F1 pause script\nF2 stop script\nLeft Mouse aim", "Hot keys", MessageBoxButton.OK, MessageBoxImage.None);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Paks();
            if (CheckBoxAim.IsChecked == true)
            {
                Process[] p = Process.GetProcessesByName("TslGame");
                if (p.Length == 0)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("If you use aimbot start PUBG before start cheat", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result == MessageBoxResult.OK)
                    {
                        CheckBoxAim.IsChecked = false;
                    }
                }
                else
                {
                    var ahk = AutoHotkeyEngine.Instance;
                    foreach (var proc in p)
                    {
                        var PidId = Convert.ToString(proc.Id);
                        string ahkPidCommand = "#NoEnv\n#SingleInstance, Force\n#Persistent\n#InstallKeybdHook\n#UseHook\n#KeyHistory, 0\n#HotKeyInterval 1\n#MaxHotkeysPerInterval 127\nPID := DllCall(\"" + PidId + "\")\nProcess, Priority, %PID%, High";
                        ahk.ExecRaw(ahkPidCommand);
                        if (File.Exists(Path.GetTempPath() + "/aim.ahk"))
                        {
                            ahk.LoadFile(Path.GetTempPath() + "/aim.ahk");
                        }
                        else
                        {
                            MessageBoxResult result = System.Windows.MessageBox.Show("Restart Launcher!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            if (result == MessageBoxResult.OK)
                            {
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }
        }

        private void CheckBoxZoom_Checked(object sender, RoutedEventArgs e)
        {
            new Radar_Launcher.Form1();
        }

        private void Paks()
        {
           

            if (CheckBoxNoRecoil.IsChecked == true)
            {
                string path = @"";
                string path2 = TextBoxPath.Text;
                File.Move(path, path2);
                //File.SetAttributes(path2, FileAttributes.System | FileAttributes.Hidden);
            }
        }

        private void TextBoxPath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            TextBoxPath.Text = dialog.SelectedPath;
        }

       
    }
}