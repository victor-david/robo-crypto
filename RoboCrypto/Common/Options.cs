using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Crypt;
using Xam.Applications.RoboCrypto.Sync;
using Xam.Applications.RoboCrypto.Resources;
using System.Reflection;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Represents the various application options specified by the user.
    /// </summary>
    public class Options
    {
        #region Public constants
        /// <summary>
        /// Gets the minimum number of bytes that must be found in the key file to perform an encrypt or decrypt operation (32).
        /// </summary>
        public const int MinimumKeyLengthBytes = 32;
        #endregion

        /************************************************************************/

        #region Private vars
        private const string EncryptOp = "/e";
        private const string DecryptOp = "/d";
        //private const string HashOp = "/h";
        private const string ForceOp1 = "/f";
        private const string ForceOp2 = "/ft";
        private const string VeboseOp = "/v";
        private const string TestOp = "/t";
        private const string NoHeaderOp = "/nh";
        private HashMode hash;
        private ForceMode force;
        #endregion

        /************************************************************************/

        #region Public Properties
        /// <summary>
        /// Gets a boolean value that indicates if there are sufficient args. 
        /// Returns true if Source, Target, KeyFile, and Mode have been specified.
        /// </summary>
        public bool HaveArgs
        {
            get
            {
                return (
                    Source != null && 
                    Target != null && 
                    KeyFile != null && 
                    (Mode == OperationMode.Encrypt || Mode == OperationMode.Decrypt));
            }
        }

        /// <summary>
        /// Gets the DirectoryInfo object for the specified source, or null if none was specified.
        /// </summary>
        public DirectoryInfo Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the DirectoryInfo object for the specified target, or null if none was specified.
        /// </summary>
        public DirectoryInfo Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the FileInfo object for the specified key file, or null if none was specified.
        /// </summary>
        public FileInfo KeyFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the operation mode, encrypt or decrypt.
        /// </summary>
        public OperationMode Mode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value from the HasMode enumeration that indicates if file and directory names will be hashed.
        /// Only applies when Mode == OperationMode.Encrypt. During decryption, returns HashMode.Auto.
        /// </summary>
        public HashMode Hash
        {
            get 
            { 
                return (Mode == OperationMode.Decrypt) ? HashMode.Auto : hash; 
            }
            private set
            {
                hash = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates if files should be encrypted whether or not 
        /// they have changed. Useful if you want to change the encryption key.
        /// If Mode is not OperationMode.Encrypt, this property returns ForceMode.None.
        /// </summary>
        public ForceMode Force
        {
            get { return (Mode == OperationMode.Encrypt) ? force : ForceMode.None; }
        }

        /// <summary>
        /// Gets a boolean value that indicates if output should be verbose.
        /// </summary>
        public bool Verbose
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a boolean value that determines if an options summary is displayed at app startup.
        /// </summary>
        public bool Header
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a boolean value that indicates if running in test mode, no changes.
        /// </summary>
        public bool Test
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public Options(string[] args)
        {
            Mode = OperationMode.None;
            /**
             * At first, Hash was optional, controlled by a command line arg.
             * Discovered problems with unhashed directories that have a period in their name
             * Don't have that problem with hashed names b/c they can't have a period.
             * So, for now at least, making it Yes all the time.
             * TODO: perhaps revisit this issue.
             */
            Hash = HashMode.Yes;
            force = ForceMode.None;
            Verbose = false;
            Header = true;
            ParseArgs(args);
        }
        #endregion

        /************************************************************************/

        #region Public Methods
        /// <summary>
        /// Gets the string to display option usage.
        /// </summary>
        /// <returns>the string</returns>
        public string OptionUsageStr()
        {
            StringBuilder b = new StringBuilder(512);
            b.AppendLine(String.Format("Usage: {0} sourceDir targetDir keyFile /e | /d [options]", Assembly.GetExecutingAssembly().GetName().Name));
            b.AppendLine(" /e  Encrypt");
            b.AppendLine(" /d  Decrypt");
            b.AppendLine(" /f  Force encryption whether source has changed or not");
            b.AppendLine(" /ft Force encryption and change timestamp on source");
            b.AppendLine(" /v  Verbose");
            b.AppendLine(" /nh Don't show the options header");
            b.AppendLine(" /t  Test, no actual changes");
            return b.ToString();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder(256);
            b.AppendLine("Options Provided:");
            b.AppendLine(String.Format("  Source: \t{0}", (Source != null) ? Source.FullName : "not specified"));
            b.AppendLine(String.Format("  Target: \t{0}", (Target != null) ? Target.FullName : "not specified"));
            b.AppendLine(String.Format("  Key File: \t{0}", (KeyFile != null) ? KeyFile.FullName : "not specified"));
            b.AppendLine(String.Format("  Mode: \t{0}", Mode));
            b.AppendLine(String.Format("  Hash: \t{0}", Hash));
            b.AppendLine(String.Format("  Force: \t{0}", Force));
            b.AppendLine(String.Format("  Verbose: \t{0}", Verbose.ToYesNo()));
            b.AppendLine(String.Format("  Test: \t{0}", Test.ToYesNo()));
            return b.ToString();
        }

        /// <summary>
        /// Validates the options. Throws if anything is incorrect.
        /// </summary>
        public void Validate()
        {
            if (!HaveArgs)
            {
                throw new ArgumentException(Strings.NotEnoughArguments);
            }

            if (!Source.Exists)
            {
                throw new InvalidOperationException(Strings.SourceNotDirectory);
            }

            if (!Target.Exists)
            {
                throw new InvalidOperationException(Strings.TargetNotDirectory);
            }

            if (Source.FullName == Target.FullName)
            {
                throw new InvalidOperationException(Strings.SourceAndTargetSame);
            }

            if (Source.Includes(Target) || Target.Includes(Source))
            {
                throw new InvalidOperationException(Strings.SourceAndTargetNested);
            }

            if (!KeyFile.Exists)
            {
                throw new FileNotFoundException(Strings.KeyFileDoesNotExist);
            }

            if (Mode == OperationMode.Decrypt && !IsTargetDirectoryEmpty())
            {
                throw new InvalidOperationException(Strings.TargetNotEmpty);
            }
        }
        #endregion

        /************************************************************************/

        #region Private Methods
        /// <summary>
        /// Parses our startup args and assigns the option values.
        /// </summary>
        /// <param name="args">The command line args</param>
        private void ParseArgs(string[] args)
        {
            if (args.Length > 0)
            {
                Source = new DirectoryInfo(args[0]);
            }

            if (args.Length > 1)
            {
                Target = new DirectoryInfo(args[1]);
            }
            
            if (args.Length > 2)
            {
                KeyFile = new FileInfo(args[2]);
            }

            if (args.Length > 3)
            {
                for (int k = 3; k < args.Length; k++)
                {
                    string arg = args[k].ToLower();
                    if (arg.StartsWith(EncryptOp)) Mode = OperationMode.Encrypt;
                    if (arg.StartsWith(DecryptOp)) Mode = OperationMode.Decrypt;
                    //if (arg.StartsWith(HashOp)) Hash = HashMode.Yes;
                    if (arg.StartsWith(ForceOp1)) force = ForceMode.ForceEncryption;
                    if (arg.StartsWith(ForceOp2)) force = ForceMode.ForceEncryptionWithTimestamp;
                    if (arg.StartsWith(VeboseOp)) Verbose = true;
                    if (arg.StartsWith(NoHeaderOp)) Header = false;
                    if (arg.StartsWith(TestOp)) Test = true;
                }
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates if the target directory is empty.
        /// </summary>
        /// <returns>true if the target directory is empty; otherwise, false.</returns>
        private bool IsTargetDirectoryEmpty()
        {
            foreach (var info in Target.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
            {
                // If the enumerate produces anything, not empty
                return false;
            }
            return true;
        }
        #endregion
    }
}