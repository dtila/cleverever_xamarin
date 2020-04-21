using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CleverEver.Logging;
using Firebase.Crash;
using System.Threading.Tasks;

namespace CleverEver.Droid.Analytics
{
    class FirebaseCrashReportingService : IExceptionLogger
    {
        static FirebaseCrashReportingService()
        {
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
#endif
        }

        public void LogError(Exception ex, string message = null, [CallerMemberName] string method = null)
        {
            while (ex != null)
            {
                InternalLogError(ex, false, message, method);
                ex = ex.InnerException;
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            InternalLogError(e.Exception, false, "TaskScheduler_UnobservedTaskException");
        }

        private static void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            InternalLogError(e.Exception, e.Handled, "AndroidEnvironment_UnhandledExceptionRaiser");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null)
                return;

            InternalLogError(ex, e.IsTerminating, "CurrentDomain_UnhandledException");
        }

        private static void InternalLogError(Exception ex, bool isFatal, string message = null, [CallerMemberName] string method = null)
        {
#if DEBUG
            Android.Util.Log.Error("CleverEver", "Message: " + message ?? "null");
            Android.Util.Log.Error("CleverEver", "Exception message: " + ex?.Message ?? "null");
            Android.Util.Log.Error("CleverEver", "Exception stacktrace: " + ex?.StackTrace ?? "null");
            System.Diagnostics.Debugger.Break();
#else
            if (message != null)
                FirebaseCrash.Log(message);
            FirebaseCrash.Report(ex);
#endif
        }
    }
}