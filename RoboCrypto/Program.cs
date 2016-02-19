using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xam.Applications.RoboCrypto.Common;
using System.Reflection;

namespace Xam.Applications.RoboCrypto
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
#if DEBUG
            finally
            {
                Console.Write("Done...");
                Console.ReadKey();
            }
#endif
        }

        /// <summary>
        /// Called from the Main entry point separately so we can catch an assembly missing.
        /// Main runs, then the runtime does a JIT for MainMain which needs other assemblies.
        /// If something is missing, the try/catch in Main handles it gracefully.
        /// </summary>
        /// <param name="args">The args</param>
        static void MainMain(string[] args)
        {
            var a = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine(String.Format("{0} v{1}", a.Name, a.Version));
            
            Options ops = new Options(args);
            if (ops.HaveArgs)
            {
                if (ops.Header)
                {
                    Console.WriteLine(ops.ToString());
                }
                MainExecution main = new MainExecution(ops);
                main.Execute();
            }
            else
            {
                Console.WriteLine(ops.OptionUsageStr());
            }
        }
    }
}
