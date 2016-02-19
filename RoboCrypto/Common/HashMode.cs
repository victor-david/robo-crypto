using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Provides an enumeration of values that determine if file / directory names are hashed.
    /// </summary>
    /// <remarks>
    /// Currently, HashMode.No is not used. Hashing is always in effect.
    /// </remarks>
    public enum HashMode
    {
        /// <summary>
        /// Hash mode is not in effect.
        /// </summary>
        No,
        /// <summary>
        /// Hash mode is in effect.
        /// </summary>
        Yes,
        /// <summary>
        /// Hash mode is automatic, set during decryption.
        /// </summary>
        Auto,
    }
}
