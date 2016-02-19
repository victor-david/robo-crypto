using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.KeyGen
{
    /// <summary>
    /// Represents the various application options specified by the user.
    /// </summary>
    public class Options
    {
        #region Constants (private)
        /// <summary>
        /// Gets the number of bytes to generate when creating a key file with small key (32).
        /// </summary>
        private const int GenerateKeyBytesCountSmall = 32;
        /// <summary>
        /// Gets the number of bytes to generate when creating a key file with medium key (64).
        /// </summary>
        private const int GenerateKeyBytesCountMedium = 64;
        /// <summary>
        /// Gets the number of bytes to generate when creating a key file with large key (128).
        /// </summary>
        private const int GenerateKeyBytesCountLarge = 128;
        /// <summary>
        /// Gets the number of bytes to generate when creating a key file with huge key (256).
        /// </summary>
        private const int GenerateKeyBytesCountHuge = 256;
        /// <summary>
        /// Gets the default number of bytes to generate when creating a key file (64).
        /// </summary>
        private const int GenerateKeyBytesCountDefault = GenerateKeyBytesCountMedium;
        #endregion

        /************************************************************************/
        
        #region Private vars
        private const string GenerateOp = "/g";
        private const string AsciiOp = "/a";
        #endregion

        /************************************************************************/

        #region Public Properties
        /// <summary>
        /// Gets the key file that was supplied.
        /// </summary>
        public string KeyFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the necessary ops were supplied. 
        /// </summary>
        public bool HaveOps
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of bytes to generate when creating a key file (option /g)
        /// </summary>
        public int GenerateKeyBytesCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of bits to generate.
        /// </summary>
        public int GenerateKeyBitsCount
        {
            get { return GenerateKeyBytesCount * 8; }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether to generate the the key with
        /// random printable ascii values only (33..126) When this property is false,
        /// the key will be generated using random byte values 0..255.
        /// </summary>
        public bool AsciiRange
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Constrcutor
        public Options(string[] args)
        {
            GenerateKeyBytesCount = GenerateKeyBytesCountDefault;
            AsciiRange = false;
            ParseArgs(args);
        }
        #endregion

        /************************************************************************/

        #region Public Methods
        /// <summary>
        /// Gets the string to display option usage.
        /// </summary>
        /// <returns>The string</returns>
        public string OptionUsageStr()
        {
            StringBuilder b = new StringBuilder(512);
            b.AppendLine("AesKeyGen - Generates a cryptographically secure key");
            b.AppendLine("Usage: AesKeyGen keyfile /g[s|m|l|h] [/a]");
            b.AppendLine("  /gs  256 bits");
            b.AppendLine("  /gm  512 bits (default)");
            b.AppendLine("  /gl  1024 bits");
            b.AppendLine("  /gh  2048 bits)");
            b.AppendLine("  /a   Use printable ascii values only (33..126), otherwise all values 0..255");
            b.AppendLine();
            b.AppendLine("If the key file exists, it will be overwritten without confirmation.");
            return b.ToString();
        }
        #endregion

        /************************************************************************/

        #region Private Methods
        private void ParseArgs(string[] args)
        {
            if (args.Length > 0)
            {
                KeyFile = Path.GetFullPath(args[0]);
                if (Directory.Exists(KeyFile))
                {
                    throw new ArgumentException("Key file refers to a directory");
                }
            }

            for (int k = 1; k < args.Length; k++)
            {
                string arg = args[k].ToLower();
                if (arg.StartsWith(GenerateOp))
                {
                    HaveOps = true;
                    if (arg.Length > GenerateOp.Length)
                    {
                        string l = arg.Substring(GenerateOp.Length, 1);
                        if (l == "s") GenerateKeyBytesCount = GenerateKeyBytesCountSmall;
                        if (l == "m") GenerateKeyBytesCount = GenerateKeyBytesCountMedium;
                        if (l == "l") GenerateKeyBytesCount = GenerateKeyBytesCountLarge;
                        if (l == "h") GenerateKeyBytesCount = GenerateKeyBytesCountHuge;
                    }
                }
                if (arg.StartsWith(AsciiOp)) AsciiRange = true;
            }
        }
        #endregion
    }
}