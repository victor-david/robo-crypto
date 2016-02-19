using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Provides some helper extensions.
    /// </summary>
    public static class HelperExtensions
    {
        /// <summary>
        /// Helper extension to return Yes or No for the boolean value
        /// </summary>
        /// <param name="b">The boolean value</param>
        /// <returns>The string Yes if b is true; otherwise, the string No.</returns>
        public static string ToYesNo(this bool b)
        {
            return (b) ? "Yes" : "No";
        }
    }
}
