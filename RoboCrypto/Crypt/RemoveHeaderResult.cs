using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Common;

namespace Xam.Applications.RoboCrypto.Crypt
{
    /// <summary>
    /// Represents the result of a RemoveHeader operation.
    /// </summary>
    public class RemoveHeaderResult
    {
        #region Public Properties
        /// <summary>
        /// Gets the original file name that was stored in the meta data header.
        /// </summary>
        public string OriginalFileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original bytes, i.e. bytes remaining after the meta data header has been removed.
        /// </summary>
        public byte[] OriginalBytes
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public RemoveHeaderResult(byte[] originalBytes, string originalFileName)
        {
            Validation.ValidateNull(originalBytes, "RemoveHeaderResult.OriginalBytes");
            Validation.ValidateNullEmpty(originalFileName, "RemoveHeaderResult.OriginalFileName");
            OriginalBytes = originalBytes;
            OriginalFileName = originalFileName;
        }
        #endregion
    }
}
