using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Enumerates the possible modes for encryption and decryption.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>
        /// No operation mode has been specified.
        /// </summary>
        None,
        /// <summary>
        /// Encryption mode.
        /// </summary>
        Encrypt,
        /// <summary>
        /// Decryption mode.
        /// </summary>
        Decrypt,
    }
}
