namespace JwlMediaWin
{
    using System;
    using System.IO;

    internal static class FileUtils
    {
        private static readonly string AppNamePathSegment = "JwlMediaWin";
        private static readonly string OptionsFileName = "options.json";

        /// <summary>
        /// Creates directory if it doesn't exist. Throws if cannot be created
        /// </summary>
        /// <param name="folderPath">Directory to create</param>
        public static void CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                if (!Directory.Exists(folderPath))
                {
                    throw new Exception($"Could not create folder {folderPath}");
                }
            }
        }

        /// <summary>
        /// Gets the log folder
        /// </summary>
        /// <returns>Log folder</returns>
        public static string GetLogFolder()
        {
            var result = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AppNamePathSegment,
                "Logs");

            CreateDirectory(result);
            return result;
        }

        /// <summary>
        /// Gets the file path for storing the user options
        /// </summary>
        /// <param name="optionsVersion">The options schema version</param>
        /// <returns>User Options file path.</returns>
        public static string GetUserOptionsFilePath(int optionsVersion)
        {
            var result = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppNamePathSegment,
                optionsVersion.ToString(),
                OptionsFileName);

            CreateDirectory(result);
            return result;
        }
    }
}
