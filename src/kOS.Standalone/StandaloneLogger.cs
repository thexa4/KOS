using System;
using System.Collections.Generic;
using System.Text;
using kOS.Safe;
using System.Diagnostics;
using kOS.Safe.Persistence;
using kOS.Safe.Compilation;
using kOS.Safe.Execution;
using kOS.Safe.Exceptions;

namespace kOS.Standalone
{
    class StandaloneLogger : ILogger
    {
        public StandaloneLogger(StandaloneSharedObjects shared)
        {
            Shared = shared;
        }

        private readonly StandaloneSharedObjects Shared;

        public void Log(string text)
        {
            Debug.WriteLine(text);
        }

        public void Log(Exception e)
        {
            Debug.WriteLine(e);

            const string LINE_RULE = "__________________________________________\n";

            string message = e.Message;

            if (kOS.Safe.Utilities.SafeHouse.Config.VerboseExceptions && e is KOSException)
            {
                // As a first primitive attempt at excercising the verbose exceptions,
                // Just use a CONFIG setting for how verbose to be.  This will need
                // to be replaced with something more sophisticated later, most likely.

                message += "\n" + LINE_RULE + "           VERBOSE DESCRIPTION\n";

                message += ((KOSException)e).VerboseMessage + "\n";

                message += LINE_RULE;

                // Take on the URL if there is one:
                string url = ((KOSException)e).HelpURL;
                if (url != String.Empty)
                    message += "\n\nMore Information at:\n" + url + "\n";
                message += LINE_RULE;
            }

            Console.Beep();

            Shared.Screen.Print(message, true);
            string traceText = TraceLog();
            Shared.Screen.Print(traceText, true);
        }

        public void LogError(string s)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string s)
        {
            Debug.WriteLine(s);
        }

        public void LogWarningAndScreen(string s)
        {
            throw new NotImplementedException();
        }

        public void SuperVerbose(string s)
        {
            Debug.WriteLine(s);
        }


        /// <summary>
        /// Return a list of strings containing the trace log of the call stack that got to
        /// the current point.
        /// </summary>
        /// <returns></returns>
        private string TraceLog()
        {
            const string BOGUS_MESSAGE = "(Cannot Show kOS Error Location - error might really be internal. See kOS devs.)";
            
            List<int> trace = Shared.Cpu.GetCallTrace();
            string msg = "";
            for (int index = 0; index < trace.Count; ++index)
            {
                Opcode thisOpcode = Shared.Cpu.GetOpcodeAt(trace[index]);
                if (thisOpcode is OpcodeBogus)
                {
                    return BOGUS_MESSAGE;
                }

                // The statement "run program" actually causes TWO nested function calls,
                // as the logic to check if the program needs compiling is implemented as a
                // separate kRISC function that gets called from the main code.  Therefore to
                // avoid the same RUN statement giving two nested levels on the call trace,
                // skip the level of the stack trace that passes through the boilerplate
                // load runner code:
                if (index > 0)
                {
                    if (thisOpcode.SourcePath == null || thisOpcode.SourcePath.VolumeId.Equals(ProgramBuilder.BuiltInFakeVolumeId))
                    {
                        continue;
                    }
                }

                string textLine = (thisOpcode is OpcodeEOF) ? "<<--EOF" : GetSourceLine(thisOpcode.SourcePath, thisOpcode.SourceLine);

                if (msg.Length == 0)
                    msg += "At ";
                else
                    msg += "Called from ";

                msg += (thisOpcode is OpcodeEOF) ? "standalone interpreter"
                    : BuildLocationString(thisOpcode.SourcePath, thisOpcode.SourceLine);
                msg += "\n" + textLine + "\n";

                int useColumn = (thisOpcode is OpcodeEOF) ? 1 : thisOpcode.SourceColumn;
                if (useColumn > 0)
                {
                    int numPadSpaces = useColumn - 1;
                    if (numPadSpaces < 0)
                        numPadSpaces = 0;
                    msg += new string(' ', numPadSpaces) + "^" + "\n";
                }
            }
            return msg;
        }

        private string BuildLocationString(GlobalPath path, int line)
        {
            if (line < 0)
            {
                // Special exception - if line number is negative then this isn't from any
                // line of user's code but from the system itself (like the triggers the compiler builds
                // to recalculate LOCK THROTTLE and LOCK STEERING each time there's an Update).
                return "(kOS built-in Update)";
            }

            return string.Format("{0}, line {1}", path, line);
        }

        private string GetSourceLine(GlobalPath path, int line)
        {
            string returnVal = "(Can't show source line)";

            if (line < 0)
            {
                // Special exception - if line number is negative then this isn't from any
                // line of user's code but from the system itself (like the triggers the compiler builds
                // to recalculate LOCK THROTTLE and LOCK STEERING each time there's an Update).
                return "<<System Built-In Flight Control Updater>>";
            }

            if (path is InternalPath)
            {
                return (path as InternalPath).Line(line);
            }

            Volume vol;

            try
            {
                vol = Shared.VolumeMgr.GetVolumeFromPath(path);
            }
            catch (KOSPersistenceException)
            {
                return returnVal;
            }

            VolumeFile file = vol.Open(path) as VolumeFile;
            if (file != null)
            {
                if (file.ReadAll().Category == FileCategory.KSM)
                    return "<<machine language file: can't show source line>>";

                string[] splitLines = file.ReadAll().String.Split('\n');
                if (splitLines.Length >= line)
                {
                    returnVal = splitLines[line - 1];
                }
            }

            return returnVal;
        }
    }
}
