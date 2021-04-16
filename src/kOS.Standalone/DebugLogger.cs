using System;
using System.Collections.Generic;
using System.Text;
using kOS.Safe;
using System.Diagnostics;

namespace kOS.Standalone
{
    class DebugLogger : ILogger
    {
        public void Log(string text)
        {
            Debug.WriteLine(text);
        }

        public void Log(Exception e)
        {
            Debug.WriteLine(e);
        }

        public void LogError(string s)
        {
            Debug.WriteLine(s);
        }

        public void LogException(Exception exception)
        {
            Debug.WriteLine(exception);
        }

        public void LogWarning(string s)
        {
            Debug.WriteLine(s);
        }

        public void LogWarningAndScreen(string s)
        {
            Debug.WriteLine(s);
        }

        public void SuperVerbose(string s)
        {
            Debug.WriteLine(s);
        }
    }
}
