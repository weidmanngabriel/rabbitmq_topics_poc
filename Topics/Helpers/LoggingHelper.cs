using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topics.Helpers
{
    public static class LoggingHelper
    {
        public static void ListenForAndLogUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled exception {(e.IsTerminating ? " (Terminating)" : "")}\n{e.ExceptionObject}");
        }
    }
}
