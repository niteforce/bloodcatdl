using bládketvindóz.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bládketvindóz
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            CreateConfigIfNotExists();
        }

        private static void CreateConfigIfNotExists()
        {
            string configFile = string.Format("{0}.config", Application.ExecutablePath);

            if (!File.Exists(configFile))
            {
                File.WriteAllText(configFile, Resources.App_Config);
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
