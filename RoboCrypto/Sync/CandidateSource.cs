using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Common;
using Xam.Applications.RoboCrypto.Crypt;
using Xam.Applications.RoboCrypto.Resources;
using SecurityDriven.Inferno;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Represents a source directory.
    /// </summary>
    public class CandidateSource : Candidate
    {
        #region Private vars
        #endregion

        /************************************************************************/

        #region Public Properties
        #endregion

        /************************************************************************/

        #region Constructor
        public CandidateSource(CandidateController controller, DirectoryInfo dir, Candidate parent, int level)
            : base(controller, dir, parent, level)
        {
        }
        #endregion

        /************************************************************************/
        
        #region Public Methods
        /// <summary>
        /// Processes the current operation.
        /// </summary>
        public override void Process()
        {
            CandidateTarget target = Controller.GetTarget(Id);
            Validation.ValidateNull(target, String.Format(Strings.TargetNotFound, Dir.FullName));
            
            target.Process();

            foreach (FileInfo f in Files)
            {
                CandidateTargetFileStatus status = target.RegisterFile(f);

                if (status != CandidateTargetFileStatus.SourceAndTargetFileSameTimeStamp || Ops.Force != ForceMode.None)
                {
                    if (Ops.Force == ForceMode.ForceEncryptionWithTimestamp)
                    {
                        f.LastWriteTimeUtc = f.LastWriteTimeUtc.AddMinutes(1);
                    }
                    Process(f, target);
                }
            }
            foreach (Candidate child in Children)
            {
                child.Process();
            }
        }
        #endregion

        /************************************************************************/

        #region Protected Methods
        /// <summary>
        /// Gets a new CandidateSource object.
        /// </summary>
        /// <param name="dir">The directory informatiom.</param>
        /// <param name="level">The nesting level.</param>
        /// <returns>A newly created CandidateSource.</returns>
        protected override Candidate GetChild(DirectoryInfo dir, int level)
        {
            return new CandidateSource(Controller, dir, this, level);
        }
        #endregion

        /************************************************************************/

        #region Private Methods
        /// <summary>
        /// Processes a single file.
        /// </summary>
        /// <param name="file">The file info.</param>
        /// <param name="target">The corresponding CandidateTarget object.</param>
        private void Process(FileInfo file, CandidateTarget target)
        {
            FileMetaData meta = new FileMetaData();

            switch (Ops.Mode)
            {
                case OperationMode.Encrypt:
                    Message.OutputIf(Strings.EncryptingFile, Ops.Test, Ops.Verbose, file.FullName);
                    if (!Ops.Test)
                    {
                        byte[] plainBytes = File.ReadAllBytes(file.FullName);
                        byte[] headeredBytes = meta.AddHeader(plainBytes, file);
                        if (headeredBytes == null)
                        {
                            Message.Output(Strings.HeaderCreationFailed, file.FullName);
                            break;
                        }
                        byte[] encryptedBytes = SuiteB.Encrypt(MasterKey, new ArraySegment<byte>(headeredBytes));
                        if (encryptedBytes != null)
                        {
                            target.Save(file, encryptedBytes);
                        }
                        else
                        {
                            Message.Output(Strings.EncryptionFailed, file.FullName);
                        }
                    }
                    break;

                case OperationMode.Decrypt:
                    if (file.Extension == CandidateTarget.EncryptedFileExtension)
                    {
                        Message.OutputIf(Strings.DecryptingFile, Ops.Test, Ops.Verbose, file.FullName);
                        if (!Ops.Test)
                        {
                            byte[] plainBytes = null;
                            ArraySegment<byte> encryptedSegment = new ArraySegment<byte>(File.ReadAllBytes(file.FullName));
                            
                            plainBytes = SuiteB.Decrypt(MasterKey, encryptedSegment);
                            if (plainBytes != null)
                            {
                                RemoveHeaderResult result = meta.RemoveHeader(plainBytes);
                                FileInfo fileOrig = new FileInfo(Path.Combine(Dir.FullName, result.OriginalFileName));
                                target.Save(fileOrig, result.OriginalBytes);
                            }
                            else
                            {
                                Message.Output(Strings.DecryptionFailed);
                            }
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
