using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecurityDriven.Inferno;
using SecurityDriven.Inferno.Extensions;
using Xam.Applications.RoboCrypto.Common;
using Xam.Applications.RoboCrypto.Crypt;
using Xam.Applications.RoboCrypto.Resources;
using System.Diagnostics;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Represents a target directory.
    /// </summary>
    public class CandidateTarget : Candidate
    {
        #region Public Fields
        public const string EncryptedFileExtension = ".aes";
        #endregion
        
        /************************************************************************/

        #region Private
        private const string EncryptedFileDirExtension = ".aesd";
        private List<FileInfo> registeredFiles;
        #endregion

        /************************************************************************/

        #region Public Properties
        #endregion

        /************************************************************************/
        
        #region Protected Properties
        #endregion

        /************************************************************************/

        #region Constructor
        public CandidateTarget(CandidateController controller, DirectoryInfo dir, Candidate parent, int level)
            : base(controller, dir, parent, level)
        {
            registeredFiles = new List<FileInfo>();
            if (IsRoot)
            {
                SubstitutedRoot = Ops.Target.FullName;
            }
            else
            {
                switch (Ops.Mode)
                {
                    case OperationMode.Encrypt:
                        SubstitutedDirectoryName = Hash.GetHashedName(SubstitutedDirectoryName, Hash.DefaultHashCharsDirectory);
                        break;
                    case OperationMode.Decrypt:
                        SubstitutedDirectoryName = GetDirectoryNameFromFile();
                        break;
                }
            }
        }
        #endregion

        /************************************************************************/
        
        #region Public Methods
        /// <summary>
        /// Registers a file for processing.
        /// </summary>
        /// <param name="source">The source file object to register.</param>
        /// <returns>
        /// A value from the CandidateTargetFileStatus enumeration that describes
        /// the target status as it relates to the specified sources.
        /// </returns>
        public CandidateTargetFileStatus RegisterFile(FileInfo source)
        {
            Validation.ValidateNull(source, "RegisterFile.Source");
            FileInfo target = ToTargetFile(source.Name, EncryptedFileExtension);
            registeredFiles.Add(target);
            if (!target.Exists) return CandidateTargetFileStatus.TargetFileDoesNotExist;
            int diff = DateTime.Compare(source.LastWriteTimeUtc, target.LastWriteTimeUtc);
            if (diff == 0) return CandidateTargetFileStatus.SourceAndTargetFileSameTimeStamp;
            if (diff > 0) return CandidateTargetFileStatus.SourceFileIsNewer;
            return CandidateTargetFileStatus.SourceFileIsOlder;
        }

        /// <summary>
        /// Processes the current operation.
        /// </summary>
        /// <param name="masterKey">The master key.</param>
        /// <remarks>
        /// This method is called by the corresponding source object one time only
        /// in order to prepare the target directory.
        /// </remarks>
        public override void Process()
        {
            if (!IsRoot)
            {
                if (!Ops.Test)
                {
                    // CreateDirectory does nothing if the directory already exists.
                    Directory.CreateDirectory(GetFullSubstitutedPath());
                }
                switch (Ops.Mode)
                {
                    case OperationMode.Encrypt:
                        FileInfo targetDirectoryFile = ToTargetFile(SubstitutedDirectoryName, EncryptedFileDirExtension);
                        registeredFiles.Add(targetDirectoryFile);
                        // This assumes that if the file exists, it's okay (unless we're in Force mode)
                        // TODO: Maybe revisit this.
                        if (targetDirectoryFile.Exists && Ops.Force == ForceMode.None) return;

                        Message.OutputIf(Strings.EncryptingFile, Ops.Test, Ops.Verbose, targetDirectoryFile.FullName);
                        if (!Ops.Test)
                        {
                            byte[] encryptedBytes = SuiteB.Encrypt(MasterKey, new ArraySegment<byte>(Dir.Name.ToBytes()));
                            if (encryptedBytes == null)
                            {
                                Message.Output(Strings.EncryptionFailed, targetDirectoryFile.FullName);
                            }
                            else
                            {
                                File.WriteAllBytes(targetDirectoryFile.FullName, encryptedBytes);
                            }
                        }
                        break;

                    case OperationMode.Decrypt:
                        /*
                         * Nothing to do on decrypt. The path was substituted from the name stored 
                         * in the encrypted file during the constructor. And before this switch statement,
                         * the substituted path was created.
                         */
                        break;
                }
            }
        }

        /// <summary>
        /// Saves the specified source file object in the target.
        /// </summary>
        /// <param name="source">The source file object to save.</param>
        /// <param name="processedBytes">The processed bytes.</param>
        public void Save(FileInfo source, byte[] processedBytes)
        {
            Validation.ValidateNull(source, "Save.Source");
            Validation.ValidateNull(processedBytes, "Save.ProcessBytes");
            FileInfo target = ToTargetFile(source.Name, (Ops.Mode == OperationMode.Encrypt) ? EncryptedFileExtension : String.Empty);

            /**
             * Get the message from resources, depending on the operation we're doing.
             */
            string message = (Ops.Mode == OperationMode.Encrypt) ? Strings.SavingEncryptedFile : Strings.SavingDecryptedFile;
            Message.OutputIf(message, Ops.Test, Ops.Verbose, target.FullName);

            /**
             * Currently, Save() is only called when Ops.Test == false, but we'll check anyway
             * in case the upstream logic changes.
             */
            if (!Ops.Test)
            {
                File.WriteAllBytes(target.FullName, processedBytes);
                target.LastWriteTimeUtc = source.LastWriteTimeUtc;
            }
        }

        /// <summary>
        /// Performs post-operation cleanup. If Ops.Mode != OperationMode.Encrypt,
        /// this method does nothing,.
        /// </summary>
        public void Cleanup()
        {
            Validation.ValidateInvalidOperation(!IsRoot, Strings.OperationMustBeRoot);

            if (Ops.Mode == OperationMode.Encrypt)
            {
                // Pass 1. Get all directories that aren't a target node, and delete them.
                DirectoryInfoList dirsToDelete = new DirectoryInfoList(Ops);

                foreach (string dir in Directory.EnumerateDirectories(Ops.Target.FullName, "*", SearchOption.AllDirectories))
                {
                    if (!DirectoryNodeExists(dir))
                    {
                        dirsToDelete.Add(new DirectoryInfo(dir));
                    }
                }
                // reverse the entries before deleting so we delete nested first.
                dirsToDelete.Reverse();
                dirsToDelete.Delete();

                // Pass 2. Build a list of extra files (unregistered) and delete them.
                List<FileInfo> filesToDelete = new List<FileInfo>();
                GetUnregisteredFiles(filesToDelete);
                foreach (var f in filesToDelete)
                {
                    Message.OutputIf(Strings.RemoveFile, Ops.Test, Ops.Verbose, f.FullName);
                    f.Delete();
                }
            }
        }
        #endregion

        /************************************************************************/
        
        #region Protected Methods
        /// <summary>
        /// Gets a new CandidateTarget object.
        /// </summary>
        /// <param name="dir">The directory informatiom.</param>
        /// <param name="level">The nesting level.</param>
        /// <returns>A newly created CandidateTarget.</returns>
        protected override Candidate GetChild(DirectoryInfo dir, int level)
        {
            return new CandidateTarget(Controller, dir, this, level);
        }
        #endregion

        /************************************************************************/

        #region Private Methods
        /// <summary>
        /// Retrieves the original directory name from the encrypted file.
        /// </summary>
        /// <returns>The original directory name.</returns>
        private string GetDirectoryNameFromFile()
        {
            string[] files = Directory.GetFiles(Dir.FullName, String.Format("*{0}", EncryptedFileDirExtension));
            if (files.Length == 1)
            {
                Message.OutputIf(Strings.RestoringDirectory, Ops.Test, Ops.Verbose, Dir.FullName);
                if (!Ops.Test)
                {
                    byte[] decryptedBytes = SuiteB.Decrypt(MasterKey, new ArraySegment<byte>(File.ReadAllBytes(files[0])));
                    Validation.ValidateNull(decryptedBytes, Strings.DecryptionFailed);
                    return decryptedBytes.FromBytes();
                }
            }
            return  Dir.Name;
        }

        /// <summary>
        /// Given a source file info object, returns the corresponding object
        /// for the target, the final full name, including the hashed directory path.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private FileInfo ToTargetFile(string fileName, string extension)
        {
            // TODO: need to revisit
            string targetFileName = (Ops.Mode == OperationMode.Encrypt) ? Hash.GetHashedName(fileName, Hash.DefaultHashCharsFileName) : fileName;
            return new FileInfo(Path.Combine(GetFullSubstitutedPath(), targetFileName + extension));
        }

        /// <summary>
        /// Gets a boolean value that indicates if the specified directory exists on this node.
        /// </summary>
        /// <param name="dir">The directory to check.</param>
        /// <returns>true if the directory exists on this node; otherwise, false.</returns>
        private bool DirectoryNodeExists(string dir)
        {
            foreach (var child in Children.OfType<CandidateTarget>())
            {
                if (child.DirectoryNodeExists(dir)) return true;
            }
            if (GetFullSubstitutedPath() == dir) return true;
            return false;
        }

        /// <summary>
        /// Recursively traverses the target tree and builds a list of any files that have not been registered.
        /// </summary>
        /// <param name="files">The list of files to add to if a file isn't registered.</param>
        /// <remarks>
        /// This method builds a list of unregistered files that are contained within a target directory.
        /// An existing file may be unregistered because the source file was renamed or deleted.
        /// This method only deals with files within a target node. If an entire directory needs to be 
        /// removed (for instance, when a source directory is renamed or deleted), that is handled on
        /// a different pass of the cleanup process.
        /// </remarks>
        private void GetUnregisteredFiles(List<FileInfo> files)
        {
            string dir = GetFullSubstitutedPath();
            foreach (var f in Directory.EnumerateFiles(dir))
            {
                if (!IsFileRegistered(f))
                {
                    files.Add(new FileInfo(f));
                }
            }
            foreach (var child in Children.OfType<CandidateTarget>())
            {
                child.GetUnregisteredFiles(files);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates if the specified file has been registered.
        /// </summary>
        /// <param name="fullName">The file name</param>
        /// <returns>true if <paramref name="fullName"/> has been registered; otherwise, false.</returns>
        private bool IsFileRegistered(string fullName)
        {
            foreach (var rf in registeredFiles)
            {
                if (fullName == rf.FullName)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
