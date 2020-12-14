using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Simple config class to hold values.
    /// </summary>
    static class Config
    {
        public static string counterStrikeInstallPath;
    }

    /// <summary>
    /// Configuration handler class.
    /// </summary>
    static class ConfigHandler
    {
        public static string configPath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\config.cfg";


        /// <summary>
        /// Read config file.
        /// </summary>
        /// <returns>If reading succeeded</returns>
        public static bool ReadConfig()
        {
            try
            {
                using (StreamReader configReader = new StreamReader(configPath))
                {
                    string configText = configReader.ReadToEnd().Trim();
                    if (!Program.IsCounterStrikeFolderValid(configText))
                        return false;
                    Config.counterStrikeInstallPath = configText;
                    return true;
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        /// <summary>
        /// Write to config file.
        /// </summary>
        /// <param name="configToWrite">Configuration to write</param>
        /// <returns>If writing succeeded</returns>
        public static bool WriteConfig(string configToWrite)
        {
            try
            {
                using (StreamWriter configStream = File.CreateText(configPath))
                {
                    configStream.Write(configToWrite);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
    }
}
