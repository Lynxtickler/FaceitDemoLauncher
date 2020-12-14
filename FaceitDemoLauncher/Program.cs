using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Main functionality class.
    /// </summary>
    static class Program
    {
        public const string CurrentVersion = "1.0.1";
        private static string[] programArgs;

        public static string compressedFilePath;
        public static string launchCommand;
        public static MessageFormCode messageFormReturnCode;

        public const string ErrorMessageTitle = "Error";
        public const string InvalidFileTypeMessage = "Only use filetype .dem.gz!";
        public const string DefaultDropAreaText = "Drop <demo>.dem.gz here";
        public const string dropAreaTextRoot = "Drop new <demo>.dem.gz here\nor\nExtract using ";

        private enum FileValidationCode { Valid, ErrorWrongType, ErrorUnknown }
        public enum MessageFormCode { Cancel, Launch }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            programArgs = args;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        /// <summary>
        /// Update compressed demo file reference and related logic.
        /// </summary>
        /// <param name="filePath">New compressed demo file path</param>
        /// <returns>If update was successful</returns>
        public static bool UpdateCompressedFile(string filePath)
        {
            FileValidationCode fileCode = ValidateDemoFormat(filePath);
            if (fileCode == FileValidationCode.ErrorWrongType)
            {
                MessageBox.Show(InvalidFileTypeMessage, ErrorMessageTitle);
                return false;
            }
            else if (fileCode == FileValidationCode.ErrorUnknown)
            {
                MessageBox.Show("Unknown error occured while processing demo file.", ErrorMessageTitle);
                return false;
            }
            Console.WriteLine("Updating compressed file path");
            compressedFilePath = filePath;
            bool success = CreateLaunchCommand();
            if (!success)
            {
                Console.WriteLine("Unexpected error occured. Launch command couldn't be generated.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate demo file type.
        /// </summary>
        /// <param name="filePath">Demo file path</param>
        /// <returns>Whether file is valid or anything is wrong with it</returns>
        private static FileValidationCode ValidateDemoFormat(string filePath)
        {
            try
            {
                if (!filePath.EndsWith(".dem.gz"))
                    return FileValidationCode.ErrorWrongType;
                return FileValidationCode.Valid;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return FileValidationCode.ErrorUnknown;
        }

        /// <summary>
        /// Create CS:GO launch command that plays demo file instantly
        /// </summary>
        /// <returns>Whether successful or not</returns>
        private static bool CreateLaunchCommand()
        {
            try
            {
                launchCommand = $"steam://rungameid/730//+playdemo {System.IO.Path.GetFileNameWithoutExtension(compressedFilePath)}";
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        /// <summary>
        /// Call methods for extracting demo file and showing the user gamelauncher dialog.
        /// </summary>
        public static bool ExtractAndShow()
        {
            string filePath = Decompress();
            if (filePath == null)
            {
                Console.WriteLine("Failed decompressing the file.");
                return false;
            }
            return StartWatchingDemo(filePath);
        }

        /// <summary>
        /// Decompress demo file to csgo-folder.
        /// </summary>
        /// <returns>Decompressed demo file path, null if decompression failed</returns>
        private static string Decompress()
        {
            try
            {
                if (Config.counterStrikeInstallPath == null)
                    return null;
                string newFileName = Path.Combine(new string[] { Config.counterStrikeInstallPath, Path.GetFileNameWithoutExtension(compressedFilePath) });
                if (DoesValidFileExist(newFileName))
                    return newFileName;
                var compressedFile = new FileInfo(compressedFilePath);
                using (FileStream compressedFileStream = compressedFile.OpenRead())
                {
                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    {
                        using (var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                            Console.WriteLine("Successfully decompressed demo file.");
                            return newFileName;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Show dialog to instantly view demo in-game or copy the appropriate console command.
        /// </summary>
        /// <param name="filePath">Demo file path</param>
        /// <returns>If user launched the game</returns>
        private static bool StartWatchingDemo(string filePath)
        {
            var messageForm = new MessageForm();
            messageForm.ShowDialog();
            if (messageFormReturnCode == MessageFormCode.Launch)
            {
                var process = new System.Diagnostics.Process();
                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = launchCommand
                };
                process.StartInfo = startInfo;
                System.Diagnostics.Process.Start("cmd", "/c start \"\" " + $"\"{launchCommand}\"");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get demo file from program arguments. Assert file exists in a legal path.
        /// </summary>
        /// <returns>True if successful</returns>
        public static bool FetchDemoFromArguments()
        {
            if (programArgs.Length < 1)
                return false;
            string demoPath = programArgs[0];
            if (DoesValidFileExist(demoPath))
                return UpdateCompressedFile(demoPath);
            return false;
        }

        /// <summary>
        /// Ask user to update csgo-folder path if one is not set.
        /// </summary>
        /// <param name="readConfig">Whether to try reading config file before asking user input</param>
        /// <returns>True if a valid path was set</returns>
        public static bool UpdateCounterStrikeInstallPath(bool readConfig = true)
        {
            if ((readConfig) && (ConfigHandler.ReadConfig()))
                return true;
            if (PickNewCounterStrikeFolder(readConfig))
                return true;
            return false;
        }



        /// <summary>
        /// Show folder dialog for choosing new csgo-folder path.
        /// </summary>
        /// <param name="showMessage">Whether to show message about missing/invalid config</param>
        /// <returns>If new folder was selected</returns>
        private static bool PickNewCounterStrikeFolder(bool showMessage=true)
        {
            FolderBrowserDialog folderPicker;
            if (showMessage)
                MessageBox.Show("Previous configuration missing or invalid.\n\nPlease, pick the folder 'csgo' located\ninside the CS:GO installation folder", "Attention");
            try
            {
                folderPicker = new FolderBrowserDialog();
                while (true)
                {
                    DialogResult result = folderPicker.ShowDialog();
                    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(folderPicker.SelectedPath))
                        return false;
                    if (IsCounterStrikeFolderValid(folderPicker.SelectedPath))
                        break;
                    MessageBox.Show("Choose a folder named 'csgo'!", ErrorMessageTitle);
                }
                Console.WriteLine("Creating new config file...");
                return ConfigHandler.WriteConfig(folderPicker.SelectedPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        /// <summary>
        /// Check if valid file exists in a valid path.
        /// </summary>
        /// <param name="path">Path to file to check</param>
        /// <returns>If file is valid</returns>
        public static bool DoesValidFileExist(string path)
        {
            if (File.Exists(path))
            {
                foreach (char c in System.IO.Path.GetInvalidPathChars())
                {
                    if (path.Contains(c)) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if selected csgo-folder is named correctly.
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <returns>If folder name is correct</returns>
        public static bool IsCounterStrikeFolderValid(string path)
        {
            return path.EndsWith("\\csgo");
        }
    }
}
