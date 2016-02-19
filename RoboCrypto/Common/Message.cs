using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xam.Applications.RoboCrypto.Common
{
    /// <summary>
    /// Provides static methods to simplify message output.
    /// </summary>
    public static class Message
    {
        /// <summary>
        /// Outputs a message if <paramref name="verbose"/> or <paramref name="test"/> is true.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="test">true of false, according to whether test option is active.</param>
        /// <param name="verbose">true of false, according to whether verbose option is active.</param>
        /// <param name="args">Optional args that can be used to substitute placeholders in <paramref name="message"/>.</param>
        public static void OutputIf(string message, bool test, bool verbose, params string[] args)
        {
            if (test || verbose)
            {
                message = String.Format(message, args);
                if (test) message = String.Format("NOP {0}", message);
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Outputs a message.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">Optional args that can be used to substitute placeholders in <paramref name="message"/>.</param>
        public static void Output(string message, params string[] args)
        {
            OutputIf(message, false, true, args);
        }
    }
}
