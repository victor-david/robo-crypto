using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xam.Applications.RoboCrypto.Sync;
using SecurityDriven.Inferno.Hash;
using SecurityDriven.Inferno.Extensions;

namespace Xam.Applications.RoboCrypto.Crypt
{
    /// <summary>
    /// Provides a single static method for creating hashed file and directory names.
    /// </summary>
    public static class Hash
    {
        #region Public Fields
        /// <summary>
        /// The default number of characters to use for a hashed file name.
        /// </summary>
        public const int DefaultHashCharsFileName = 24;
        /// <summary>
        /// The default number of characters to use for a hashed directory name.
        /// </summary>
        public const int DefaultHashCharsDirectory = 16;
        #endregion

        /************************************************************************/
        
        #region Public Methods
        /// <summary>
        /// Creates a hashed string using SHA256 of the specified string input.
        /// </summary>
        /// <param name="input">The string input to hash.</param>
        /// <param name="max">The max number of hash characters to return, values less than 16 will be adjusted upward.</param>
        /// <returns>The hashed string of the specified input.</returns>
        public static string GetHashedName(string input, int max = 64)
        {
            // byte[] bytes = HashFactories.SHA256().ComputeHash(Encoding.UTF8.GetBytes(input));
            byte[] bytes = HashFactories.SHA256().ComputeHash(input.ToBytes());

            StringBuilder sb = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            max = Math.Min(max, sb.Length);
            max = Math.Max(max, 16);

            return sb.ToString(0, max);
        }
        #endregion
    }
}
