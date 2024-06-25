namespace CLASSICCore
{
    public static class FileUtilities
    {
        public static void RemoveReadOnly(string filePath)
        {
            try
            {
                // Check if file exists
                if (!File.Exists(filePath))
                {
                    ClassicLogger.LogError($"Error: '{filePath}' not found.");
                    return;
                }

                // Get file attributes
                FileAttributes attributes = File.GetAttributes(filePath);

                // Check if file is read-only
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Remove read-only attribute
                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, attributes);

                    ClassicLogger.LogInfo($"'{filePath}' is no longer read-only.");
                }
                else
                {
                    ClassicLogger.LogInfo($"'{filePath}' is not set to read-only.");
                }
            }
            catch (FileNotFoundException ex)
            {
                ClassicLogger.LogError($"Error: '{filePath}' not found. {ex.Message}");
            }
            catch (Exception ex)
            {
                ClassicLogger.LogError($"Error: {ex.Message}");
            }
        }
    }
}