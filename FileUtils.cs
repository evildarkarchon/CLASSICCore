using Ude;
using System.Text;
namespace CLASSICCore
{
    public static class FileUtils
    {
        public static StreamReader OpenFileWithEncoding(string filePath)
        {
            // Read the file as bytes
            byte[] rawData = File.ReadAllBytes(filePath);

            // Detect the encoding
            CharsetDetector detector = new CharsetDetector();
            detector.Feed(rawData, 0, rawData.Length);
            detector.DataEnd();

            // Get the detected encoding
            string encodingName = detector.Charset ?? "UTF-8";
            Encoding encoding = Encoding.GetEncoding(encodingName);

            // Open the file with the detected encoding
            return new StreamReader(filePath, encoding, true);
        }
    }
}