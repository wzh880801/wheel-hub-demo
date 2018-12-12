using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelhubDemo
{
    public class AppConfig
    {
        public static string ApiUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            }
        }

        public static float Threshold
        {
            get
            {
                return float.Parse(System.Configuration.ConfigurationManager.AppSettings["Threshold"]);
            }
        }
    }
}
