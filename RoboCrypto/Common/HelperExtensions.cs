using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        /// <summary>
        /// Gets a boolean value that indicates if directory <paramref name="a"/> includes directory <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The outer directory</param>
        /// <param name="b">The inner directory to check</param>
        /// <returns>true if directory <paramref name="b"/> is inside of directory <paramref name="a"/>; otherwise, false.</returns>
        public static bool Includes(this DirectoryInfo a, DirectoryInfo b)
        {
            foreach (var subDir in a.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                if (subDir.FullName == b.FullName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
