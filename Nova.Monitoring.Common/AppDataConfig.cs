using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.Monitoring.Common
{
    public class AppDataConfig
    {
        public static readonly string MonitorBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"NovaLCT 2012\Config\Monitoring\");

        public static readonly string DataBaseFilePath = System.IO.Path.Combine(MonitorBasePath, "MonitorDb.db");
        public static readonly string SERVER_PATH = AppDomain.CurrentDomain.BaseDirectory + "..\\MarsServerProvider\\MarsServerProvider.exe";
        public static readonly string SERVER_NAME = "MarsServerProvider";
        public static readonly string LCT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\NovaLCT-Mars.exe");
        public static readonly string LCT_Name = "NovaLCT-Mars";
        public static readonly string RAMTable_PATH = AppDomain.CurrentDomain.BaseDirectory + "..\\CommonData\\RamTable.bin";
        public static string CurrentMonitorVersion = string.Empty;
        public static string CurrentLCTVersion = string.Empty;
        public static string CurrentM3Version = string.Empty;
        public static string CurrentMAC = string.Empty;
    }
}
