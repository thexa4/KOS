using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kOS.Suffixed;
using kOS.Compilation;
using kOS.Persistence;

namespace kOS.Execution
{
    public enum ProgramStatus
    {
        Running = 1,
        Waiting = 2,
        NotStarted = 3
    }

    public class kThread : SpecialValue
    {
        private SharedObjects _shared;
        private ContextManager _contextManager;
        private Stack _stack;
        private ProgramFile _file = null;
        private bool _programStarted;
        private object[] _parameters;

        public bool RunsInBackground { get; private set; }
        public string ProgramName { get; private set; }
        public bool Enabled { get; private set; }
        public ProgramContext Context { get { return _contextManager.CurrentContext; } }
        public ProgramStatus Status { get; set; }
        public double TimeWaitUntil { get; set; }
        public List<kThread> ChildThreads { get; private set; }

        public kThread(SharedObjects shared)
        {
            _shared = shared;
            _contextManager = new ContextManager(_shared);
            _stack = new Stack();
            Status = ProgramStatus.NotStarted;
            ChildThreads = new List<kThread>();
        }

        public kThread(SharedObjects shared, string programName, object[] parameters)
            : this(shared)
        {
            ProgramName = programName;
            _parameters = parameters;
            RunsInBackground = true;
            LoadFile();
        }

        // preload the file because we don't know if the volume
        // is going to be available when we start the thread
        private void LoadFile()
        {
            if (_shared.VolumeMgr != null)
            {
                if (_shared.VolumeMgr.CurrentVolume == null) throw new Exception("Volume not found");
                _file = _shared.VolumeMgr.CurrentVolume.GetByName(ProgramName);
                if (_file == null) throw new Exception(string.Format("File '{0}' not found", ProgramName));
            }
        }

        public override bool SetSuffix(string suffixName, object value)
        {
            switch (suffixName)
            {
                case "START":
                    Start();
                    return true;
                case "STOP":
                    Stop();
                    return true;
            }

            return base.SetSuffix(suffixName, value);
        }

        public void Start()
        {
            Enabled = true;
            if (!_programStarted && _file != null)
                StartProgram();
        }

        public void Stop()
        {
            Enabled = false;
        }

        private void StartProgram()
        {
            List<CodePart> parts = _shared.ScriptHandler.Compile(_file.Content);
            ProgramBuilder builder = new ProgramBuilder();
            builder.AddRange(parts);
            List<Opcode> program = builder.BuildProgram(false);

            if (program.Count > 0)
            {
                // push the program's arguments in this thread stack
                foreach (object parameter in _parameters)
                {
                    PushStack(parameter);
                }
                
                RunProgram(program, false);
            }
        }
        
        public void RunProgram(List<Opcode> program, bool silent)
        {
            if (program.Count > 0)
            {
                ProgramContext newContext = new ProgramContext(program);
                newContext.Silent = silent;
                _contextManager.PushContext(newContext);
                Status = ProgramStatus.Running;
                _programStarted = true;
            }
        }

        public void UpdateProgram(List<Opcode> program)
        {
            if (program.Count > 0)
            {
                if (Context != null && Context.Program != null)
                {
                    Context.UpdateProgram(program);
                }
                else
                {
                    RunProgram(program, false);
                }
            }
        }

        public void BreakExecution()
        {
            _contextManager.PopContext();
        }

        public bool HasProgramRunning()
        {
            return (_contextManager.Count > 0);
        }

        public void OnRemove()
        {
            _contextManager.PopContext();
        }

        public void PushStack(object item)
        {
            _stack.Push(item);
        }

        public object PopStack()
        {
            return _stack.Pop();
        }

        public void MoveStackPointer(int delta)
        {
            _stack.MoveStackPointer(delta);
        }
    }
}
