using System;
using System.IO;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Static Utils class
    /// </summary>
    public static class Utils
    {
        private const string rndCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private static Random rnd = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Writes a logfile entry
        /// </summary>
        /// <param name="logFile">Path to the logfile</param>
        /// <param name="headerValue">Header as string. The values are separated by tab</param>
        /// <param name="value">Value or values to write. Use tab in case of several values to separate them</param>
        /// <returns>True if the entry could be written, otherwise false</returns>
        public static bool Log(string logFile, string headerValue, string value)
        {
            try
            {
                bool exists = false;
                if (File.Exists(logFile) == true) { exists = true; }
                FileStream fs = new FileStream(logFile, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                if (exists == false)
                {
                    sw.WriteLine(headerValue);
                }
                sw.WriteLine(value);
                sw.Flush();
                fs.Flush();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("The logfile: " + logFile + " could not be written\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Converts a byte array into a hex string
        /// </summary>
        /// <param name="input">Byte array to convert</param>
        /// <returns>Hex string (e.g. '0AF3')</returns>
        public static string ConvertBytesToString(byte[] input)
        {
            string code = BitConverter.ToString(input);
            return code.Replace("-", "");
        }

        /// <summary>
        /// Converts a single byte into a hex string
        /// </summary>
        /// <param name="input">Byte to convert</param>
        /// <returns>Hex string (e.g. '00', '51' or 'BC')</returns>
        public static string ConvertBytesToString(byte input)
        {
            byte[] b = new byte[] { input };
            return ConvertBytesToString(b);
        }

        /// <summary>
        /// Gets a random alphanumeric string of the defined length
        /// </summary>
        /// <param name="length">Length of the string in characters</param>
        /// <returns>Random String</returns>
        public static string GetRandomString(int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = rndCharacters[rnd.Next(rndCharacters.Length)];
            }
            return new string(chars);
        }

    }
}
