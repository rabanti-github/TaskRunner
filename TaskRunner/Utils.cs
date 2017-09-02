using System;
using System.IO;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Static Utils class
    /// </summary>
    public static class Utils
    {
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
    }
}
