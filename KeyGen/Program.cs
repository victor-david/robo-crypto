using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecurityDriven.Inferno;
using System.IO;

namespace Xam.Applications.RoboCrypto.KeyGen
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MainMain(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Called from the Main entry point separately so we can catch an assembly missing.
        /// Main runs, then the runtime does a JIT for MainMain which needs other assemblies.
        /// If something is missing, the try/catch in Main handles it gracefully.
        /// </summary>
        /// <param name="args">The args</param>
        static void MainMain(string[] args)
        {
            Options ops = new Options(args);

            if (ops.HaveOps)
            {
                CryptoRandom cr = new CryptoRandom();
                int lower = (ops.AsciiRange) ? 33 : 0;
                int upper = (ops.AsciiRange) ? 127 : 256;

                using (MemoryStream mem = new MemoryStream())
                {
                    while (mem.Length < ops.GenerateKeyBytesCount)
                    {
                        
                        mem.WriteByte((byte)cr.Next(lower, upper));
                    }
                    File.WriteAllBytes(ops.KeyFile, mem.ToArray());
                }

                Console.WriteLine(String.Format("Created key file: {0} with {1} bits", ops.KeyFile, ops.GenerateKeyBitsCount));

            }
            else
            {
                Console.WriteLine(ops.OptionUsageStr());
            }
        }


    }
}
