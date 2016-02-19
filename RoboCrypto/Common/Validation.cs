using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Provides static helper methods to use for validation.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Throws an ArgumentNullException if the specified object is null.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="msg">A message.</param>
        public static void ValidateNull(object obj, string msg)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(msg);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the specified string is null or empty
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="msg">A message.</param>
        public static void ValidateNullEmpty(string str, string msg)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(msg);
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException if the specified condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="msg">The message</param>
        public static void ValidateInvalidOperation(bool condition, string msg)
        {
            if (condition)
            {
                throw new InvalidOperationException(msg);
            }
        }
    }
}
