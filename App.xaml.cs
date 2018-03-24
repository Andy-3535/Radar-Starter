using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.System_Windows_Interactivity);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.MahApps_Metro);
            dllFilesBytes.Add(Radar_Launcher.Properties.Resources.ControlzEx);
            int n = 0;
            while (n <= 2)
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
