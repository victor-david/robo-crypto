using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SecurityDriven.Inferno;
using Xam.Applications.RoboCrypto.Crypt;
using Xam.Applications.RoboCrypto.Resources;
using Xam.Applications.RoboCrypto.Sync;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Represents the main execution entry point after startup, setting of options, etc.
    /// </summary>
    public class MainExecution
    {
        #region Private Vars
        private Options ops;
        #endregion

        /************************************************************************/

        #region Constructor
        public MainExecution(Options ops)
        {
            Validation.ValidateNull(ops, "MainExecution.Ops");
            this.ops = ops;
        }
        #endregion

        /************************************************************************/
        
        #region Public Methods
        /// <summary>
        /// Runs the operation that was specified at startup.
        /// </summary>
        public void Execute()
        {
            byte[] masterKey = File.ReadAllBytes(ops.KeyFile.FullName);

            var controller = CandidateController.GetController(ops, masterKey);
            controller.Process();
            controller.Cleanup();
        }
        #endregion

        /************************************************************************/

        #region Private Methods (none)
        #endregion
    }
}
