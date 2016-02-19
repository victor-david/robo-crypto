using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Provides an enumeration of values that may used when forcing encryption.
    /// </summary>
    public enum ForceMode
    {
        /// <summary>
        /// None. Force mode is not active.
        /// </summary>
        None,
        /// <summary>
        /// Force encryption whether source files have changed or not.
        /// </summary>
        ForceEncryption,
        /// <summary>
        /// Force encryption and change the last-modified timestamp of the source.
        /// </summary>
        ForceEncryptionWithTimestamp,
    }
}
