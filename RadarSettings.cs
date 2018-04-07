using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using ApplicationUpdate;
using System.Windows;

namespace Radar_Settings
{
    class SettingsRad
    {
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
