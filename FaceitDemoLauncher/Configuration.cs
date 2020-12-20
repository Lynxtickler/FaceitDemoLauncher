using System;
using System.IO;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Simple config class to hold values.
    /// </summary>
    static class Config
    {
        private static string counterStrikeInstallPath;

        public static string CounterStrikeInstallPath { get => counterStrikeInstallPath; set => counterStrikeInstallPath = value; }
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
        /// <param name="showErrorMessages">Whether to show error messages or not</param>
        /// <returns>If reading succeeded</returns>
        public static bool ReadConfig(bool showErrorMessages=true)
        {
            try
            {
                using (StreamReader configReader = new StreamReader(configPath))
                {
                    string configText = configReader.ReadToEnd().Trim();
                    if (!Program.IsCounterStrikeFolderValid(configText))
                        return false;
                    Config.CounterStrikeInstallPath = configText;
                    return true;
                }
            }
            catch (Exception e)
            {
                if ((e is FileNotFoundException) || (e is DirectoryNotFoundException))
                {
                    if (showErrorMessages)
                        Program.ShowErrorBox("Config file not found.");
                }
                else if ((e is ArgumentException) || (e is ArgumentNullException) || (e is IOException))
                {
                    if (showErrorMessages)
                        Program.ShowErrorBox("Config file is not readable.");
                }
                else
                {
                    Console.WriteLine(e);
                    Program.ShowErrorBox(e.Message, true);
                }
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
                if ((e is ArgumentException) || (e is ArgumentNullException) || (e is IOException))
                {
                    Program.ShowErrorBox("Config file is not writable.");
                }
                else if ((e is DirectoryNotFoundException) || (e is PathTooLongException))
                {
                    Program.ShowErrorBox("Config file not found.");
                }
                else if ((e is UnauthorizedAccessException) || (e is System.Security.SecurityException))
                {
                    Program.ShowErrorBox("Insufficient permissions for writing to this install directory.");
                }
                else
                {
                    Console.WriteLine(e);
                    Program.ShowErrorBox(e.Message, true);
                }
            }
            return false;
        }
    }
}
