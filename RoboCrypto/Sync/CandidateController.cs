using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Common;
using System.Security.Cryptography;
using Xam.Applications.RoboCrypto.Resources;
using System.IO;

namespace Xam.Applications.RoboCrypto.Sync
{
    /// <summary>
    /// Represents the candidate controller, creates source and target objects,
    /// provides access to common properties, and provides functionality for
    /// a source object to locate its corresponding target.
    /// </summary>
    public sealed class CandidateController
    {
        #region Private Vars
        private static CandidateController instance;
        private CandidateSource sourceRoot;
        private CandidateTarget targetRoot;
        #endregion

        /************************************************************************/

        #region Public Properties
        /// <summary>
        /// Gets the options object.
        /// </summary>
        public Options Ops
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the master key used for encryption and decryption.
        /// </summary>
        public byte[] MasterKey
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Static Access / Private Constructor 
        /// <summary>
        /// Gets the singleton instance of the CandidateController.
        /// </summary>
        /// <param name="ops">The Options object.</param>
        /// <param name="masterKey">The master key.</param>
        /// <returns>The singleton instance of the CandidateController.</returns>
        public static CandidateController GetController(Options ops, byte[] masterKey)
        {
            if (instance == null)
            {
                instance = new CandidateController(ops, masterKey);
            }
            return instance;
        }

        /// <summary>
        /// Creates a new instance of the CandidateController. Private constructor.
        /// </summary>
        /// <param name="ops">The Options object.</param>
        /// <param name="masterKey">The master key.</param>
        private CandidateController(Options ops, byte[] masterKey)
        {
            Validation.ValidateNull(ops, "CandidateController.Ops");
            Validation.ValidateNull(masterKey, "CandidateController.MasterKey");

            if (masterKey.Length < Options.MinimumKeyLengthBytes)
            {
                throw new CryptographicException(Strings.KeyIsTooShort);
            }

            Ops = ops;
            Ops.Validate();
            MasterKey = masterKey;

            sourceRoot = new CandidateSource(this, ops.Source, null, 0);
            targetRoot = new CandidateTarget(this, ops.Source, null, 0);
        }
        #endregion

        /************************************************************************/

        #region Public Methods
        /// <summary>
        /// Gets the target object with the specified id.
        /// </summary>
        /// <param name="id">The id of the desired target.</param>
        /// <returns>The CandidateTarget object with id of <paramref name="id"/>, or null if not found.</returns>
        public CandidateTarget GetTarget(int id)
        {
            return (CandidateTarget)targetRoot.GetById(id);
        }

        /// <summary>
        /// Processes the operation as specified in the startup options.
        /// </summary>
        public void Process()
        {
            sourceRoot.Process();
        }

        /// <summary>
        /// Performs post-operation cleanup.
        /// </summary>
        public void Cleanup()
        {
            targetRoot.Cleanup();
        }

#if DEBUG
        /// <summary>
        /// Gets a friendly debug string
        /// </summary>
        /// <returns>A string that shows the source and the target</returns>
        public override string ToString()
        {
            return
                sourceRoot.ToString() +
                "=================================================\n\n" + 
                targetRoot.ToString();
        }
#endif
        #endregion
    }
}
