using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        List<string> dllFilesPath = new List<string>();
        List<byte[]> dllFilesBytes = new List<byte[]>();


        public App() : base()
        {
            dllFilesPath.Add(Environment.CurrentDirectory + "/System.Windows.Interactivity.dll");
            dllFilesPath.Add(Environment.CurrentDirectory + "/MahApps.Metro.dll");
            dllFilesPath.Add(Environment.CurrentDirectory + "/ControlzEx.dll");
            dllFilesPath.Add(Environment.CurrentDirectory + "/arpspoof.exe");
            dllFilesPath.Add(Environment.CurrentDirectory + "/Ionic.Zip.dll");
            dllFilesPath.Add(Environment.CurrentDirectory + "/app.version");
            dllFilesPath.Add(Environment.CurrentDirectory + "/ApplicationUpdate.dll");
            dllFilesPath.Add(Environment.CurrentDirectory + "/AutoHotkey.Interop.dll");
            dllFilesPath.Add(Path.GetTempPath() + "/aim.ahk");
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.System_Windows_Interactivity);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.MahApps_Metro);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.ControlzEx);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.arpspoof);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.Ionic_Zip);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.app);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.ApplicationUpdate);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.AutoHotkey_Interop);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.aim);
            int n = 0;
            while (n <= 8)
            {
                using (FileStream fs = File.Create(dllFilesPath[n]))
                {
                    Byte[] info = dllFilesBytes[n];
                    fs.Write(info, 0, info.Length);
                    n++;
                }
            }
        }
    }
}
