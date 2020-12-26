using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace FaceitDemoLauncher
{
    enum FileValidationCode { Valid, ErrorWrongType, ErrorUnknown }


    /// <summary>
    /// Main functionality class.
    /// </summary>
    static class Program
    {
        // Properties
        public static string Version => Application.ProductVersion;
        public static string CompressedFilePath { get => compressedFilePath; set => compressedFilePath = value; }

        // Fields
        private static string[] programArgs;
        private static string compressedFilePath;
        private static string launchCommand;

        // Constants
        public const string AcceptedSpecialCharacters = " .";
        public const string ErrorMessageTitle = "Error";
        public const string InvalidFileTypeMessage = "Only use filetype .dem.gz!";
        public const string DefaultDropAreaText = "Drop <demo>.dem.gz here";
        public const string dropAreaTextRoot = "Drop new <demo>.dem.gz here\nor\nExtract using ";


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
        /// Show error message
        /// </summary>
        /// <param name="errorMessage">Message to show</param>
        /// <param name="unexpected">If error was unexpected</param>
        public static void ShowErrorBox(string errorMessage, bool unexpected = false)
        {
            if (unexpected)
                errorMessage = "Unexpected error:\n" + errorMessage;
            MessageBox.Show(errorMessage, Program.ErrorMessageTitle, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
            CompressedFilePath = filePath;
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
                ShowErrorBox(e.Message, unexpected: true);
            }
            return FileValidationCode.ErrorUnknown;
        }

        /// <summary>
        /// Create CS:GO launch command that plays demo file instantly
        /// </summary>
        /// <param name="demoName">Demo file name</param>
        /// <returns>Whether successful or not</returns>
        private static bool CreateLaunchCommand(string demoName = null)
        {
            try
            {
                if (demoName == null)
                    demoName = System.IO.Path.GetFileNameWithoutExtension(CompressedFilePath);
                launchCommand = $"steam://rungameid/730//+playdemo {demoName}";
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowErrorBox(e.Message, unexpected: true);
            }
            return false;
        }

        /// <summary>
        /// Call methods for extracting demo file and showing the user gamelauncher dialog.
        /// </summary>
        /// <param name="newFileName">Optional name for decompressed demo file</param>
        public static bool ExtractAndShow(string newFileName = null)
        {
            string filePath = Decompress(newFileName);
            if (filePath == null)
            {
                Console.WriteLine("Failed decompressing the file.");
                return false;
            }
            return StartWatchingDemo(Path.GetFileName(filePath));
        }

        /// <summary>
        /// Decompress demo file to csgo-folder.
        /// </summary>
        /// <param name="newFileName">Optional new name for decompressed file</param>
        /// <returns>Decompressed demo file path, null if decompression failed</returns>
        private static string Decompress(string newFileName = null)
        {
            // TODO: needs refactoring
            if (Config.CounterStrikeInstallPath == null)
            {
                ShowErrorBox("Configuration hasn't been saved in memory", unexpected: true);
                return null;
            }
            string newFilePath;
            if (IsFileValid(newFileName))
            {
                if (!newFileName.EndsWith(".dem"))
                    newFileName = newFileName + ".dem";
                newFilePath = Path.Combine(new string[] { Config.CounterStrikeInstallPath, newFileName });
            }
            else
            {
                newFilePath = Path.Combine(new string[] { Config.CounterStrikeInstallPath, Path.GetFileNameWithoutExtension(CompressedFilePath) });
            }
            if (DoesValidFileExist(newFilePath))
                return newFilePath;
            FileInfo compressedFile = null;
            FileStream compressedFileStream = null;
            FileStream decompressedFileStream = null;
            try
            {
                compressedFile = new FileInfo(CompressedFilePath);
                compressedFileStream = compressedFile.OpenRead();
                decompressedFileStream = File.Create(newFilePath);
                using (var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                    Console.WriteLine("Successfully decompressed demo file.");
                    return newFilePath;
                }

            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                    ShowErrorBox($"Either \"{CompressedFilePath}\"\nor\n\"{newFilePath}\" was not found.");
                else if ((e is UnauthorizedAccessException) || (e is IOException) || (e is PathTooLongException) || (e is ArgumentException) || (e is ArgumentNullException) || (e is System.Security.SecurityException))
                    ShowErrorBox($"Either \"{CompressedFilePath}\"\nor\n\"{newFilePath}\" is not readable/writable.");
                else
                    ShowErrorBox(e.Message, unexpected: true);
                return null;
            }
            finally
            {
                if (decompressedFileStream != null)
                    decompressedFileStream.Dispose();
            }
        }

        /// <summary>
        /// Show dialog to instantly view demo in-game or copy the appropriate console command.
        /// </summary>
        /// <param name="demoName">Demo file name</param>
        /// <returns>If user launched the game</returns>
        private static bool StartWatchingDemo(string demoName)
        {
            bool success = CreateLaunchCommand(demoName);
            if (!success)
            {
                ShowErrorBox("Launch command couldn't be created.", unexpected: true);
                return false;
            }
            var messageForm = new MessageForm(demoName);
            DialogResult dialogResult = messageForm.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                RunStartInHiddenCmd(launchCommand);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Open hidden Command Prompt window and pass command as param to start-command line utility.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if nothing unexpected happened</returns>
        public static bool RunStartInHiddenCmd(string command)
        {
            try
            {
                var process = new System.Diagnostics.Process();
                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/c start \"\" " + $"\"{command}\""
                };
                process.StartInfo = startInfo;
                process.Start();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowErrorBox(e.Message,unexpected: true);
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
        public static bool UpdateCounterStrikeInstallPath(bool readConfig=true)
        {
            if ((readConfig) && (ConfigHandler.ReadConfig(showErrorMessages: false)))
                return true;
            string defaultCounterStrikeFolder = FindDefaultCounterStrikeFolder();
            if (defaultCounterStrikeFolder != null)
            {
                if (ConfigHandler.WriteConfig(defaultCounterStrikeFolder))
                    return true;
            }
            if (PickNewCounterStrikeFolder(showMessage: readConfig))
                return true;
            return false;
        }

        /// <summary>
        /// Check most common install location for Counter-Strike: Global Offensive on multiple drives.
        /// </summary>
        /// <returns>Path if existing folder found, null if not</returns>
        private static string FindDefaultCounterStrikeFolder()
        {
            var driveLetters = new string[] { "C", "D", "E", "F" };
            var pathBase = @"Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
            foreach (var driveLetter in driveLetters)
            {
                var currentDrivePath = Path.Combine(new string[] { driveLetter + @":\", pathBase });
                if ((IsCounterStrikeFolderValid(currentDrivePath)) && (Directory.Exists(currentDrivePath)))
                {
                    return currentDrivePath;
                }
            }
            return null;
        }

        /// <summary>
        /// Show folder dialog for choosing new csgo-folder path.
        /// </summary>
        /// <param name="showMessage">Whether to show message about missing/invalid config</param>
        /// <returns>If new folder was selected</returns>
        private static bool PickNewCounterStrikeFolder(bool showMessage=true)
        {
            if (showMessage)
                MessageBox.Show("Previous configuration missing or invalid.\n\nPlease, pick the folder 'csgo' located\ninside the CS:GO installation folder", "Create new config", MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
            FolderBrowserDialog folderPicker;
            try
            {
                folderPicker = new FolderBrowserDialog();
                while (true)
                {
                    DialogResult result = folderPicker.ShowDialog();
                    if ((result != DialogResult.OK) || (string.IsNullOrWhiteSpace(folderPicker.SelectedPath)))
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
                ShowErrorBox(e.Message, unexpected: true);
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
            if ( (File.Exists(path)) && (IsPathValid(path)) )
                return true;
            return false;
        }

        /// <summary>
        /// Check if a whole path is valid in theory. Does not take drive letters into consideration.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>If path is valid in theory</returns>
        private static bool IsPathValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            return (path.IndexOfAny(Path.GetInvalidPathChars()) < 0);
        }

        /// <summary>
        /// Check if file name is valid.
        /// </summary>
        /// <param name="fileName">File name to check</param>
        /// <returns>If file name is valid</returns>
        private static bool IsFileValid(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;
            return (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0);
        }

        /// <summary>
        /// Check if selected csgo-folder is named correctly.
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <returns>If folder name is correct</returns>
        public static bool IsCounterStrikeFolderValid(string path)
        {
            return ((IsPathValid(path)) && (path.EndsWith("\\csgo")));
        }

        /// <summary>
        /// Check if string is completely made of English letters, numerals or a few common special characters
        /// </summary>
        /// <param name="stringToBeChecked">String to be checked</param>
        /// <returns>Is string good for a Counter-Strike demo name</returns>
        public static bool IsStringValidDemoName(string stringToBeChecked)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(stringToBeChecked, $"^[a-zA-Z0-9{AcceptedSpecialCharacters}]*$");
        }
    }
}
