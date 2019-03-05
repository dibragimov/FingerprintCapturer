using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace HorioFingerprintCapturer
{
    public class Settings
    {
        private static readonly System.Configuration.Configuration Config = ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);

        private static string ExecutableFilePath
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(Settings)).Location;
            }
        }

        public static string ConnectionString
        {
            get
            {
                return Config.ConnectionStrings.ConnectionStrings["ConnectionString"].ConnectionString;                
            }
        }

        public static string CurrentCulture
        {
            get
            {
                return Config.AppSettings.Settings["Language"].Value;
            }
        }

        public static int MinimalTemplateQualityPercent
        {
            get
            {
                return Convert.ToInt32(Config.AppSettings.Settings["MinimalTemplateQualityPercent"].Value);
            }
        }
    }
}
