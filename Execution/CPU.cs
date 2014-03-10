using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using kOS.Suffixed;
using kOS.Function;
using kOS.Compilation;

namespace kOS.Execution
{
    public class CPU: IUpdateObserver
    {
        private Stack _stack;
        private Dictionary<string, Variable> _vars;
        private double _currentTime;
        private Dictionary<string, FunctionBase> _functions;
        private SharedObjects _shared;
        private ThreadManager _threadManager;
        private kThread _currentThread;
        private kThread _interpreterThread;
        
        public int InstructionPointer
        {
            get { return _currentThread.Context.InstructionPointer; }
            set { _currentThread.Context.InstructionPointer = value; }
        }

        public CPU(SharedObjects shared)
        {
            _shared = shared;
            _shared.Cpu = this;
            _stack = new Stack();
            _vars = new Dictionary<string, Variable>();
            if (_shared.UpdateHandler != null) _shared.UpdateHandler.AddObserver(this);
            Boot();
        }

        private void LoadFunctions()
        {
            _functions = new Dictionary<string, FunctionBase>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                FunctionAttribute attr = (FunctionAttribute)type.GetCustomAttributes(typeof(FunctionAttribute), true).FirstOrDefault();
                if (attr != null)
                {
                    object functionObject = Activator.CreateInstance(type);
                    foreach (string functionName in attr.Names)
                    {
                        if (functionName != string.Empty)
                        {
                            _functions.Add(functionName, (FunctionBase)functionObject);
                        }
                    }
                }
            }
        }

        public void Boot()
        {
            // clear stack
            _stack.Clear();
            // clear variables
            _vars.Clear();
            // clear interpreter
            if (_shared.Interpreter != null) _shared.Interpreter.Reset();
            // load functions
            LoadFunctions();
            // load bindings
            if (_shared.BindingMgr != null) _shared.BindingMgr.LoadBindings();
            // start a new thread manager
            _threadManager = new ThreadManager(_shared);
            // start a new thread for the interpreter
            StartInterpreterThread();
            // Booting message
            if (_shared.Screen != null)
            {
                _shared.Screen.ClearScreen();
                string bootMessage = "kRISC Operating System\n" +
                                     "KerboScript v" + Core.VersionInfo.ToString() + "\n \n" +
                                     "Proceed.\n ";
                _shared.Screen.Print(bootMessage);
            }
        }

        private void StartInterpreterThread()
        {
            _interpreterThread = _threadManager.CreateThread();
            _interpreterThread.Start();
            _currentThread = _interpreterThread;
        }

        public void RunProgram(List<Opcode> program)
        {
            RunProgram(program, false);
        }

        public void RunProgram(List<Opcode> program, bool silent)
        {
            // if the current thread belongs to the interpreter
            // then start a new one for the program
            if (_currentThread == _interpreterThread)
            {
                // stop the interpreter thread while a program is running
                _interpreterThread.Stop();
                // start a new thread for the program
                kThread programThread = _threadManager.CreateThread();
                programThread.RunProgram(program, silent);
                programThread.Start();
            }
            else
            {
                _currentThread.RunProgram(program, silent);
            }
        }

        public void UpdateProgram(List<Opcode> program)
        {
            _currentThread.UpdateProgram(program);
        }

        public void BreakExecution(bool manual)
        {
            if (_currentThread != _interpreterThread)
            {
                EndWait();

                if (manual)
                {
                    _threadManager.Remove(_currentThread);
                    _interpreterThread.Start();
                    _shared.Screen.Print("Program aborted.");
                }
                else
                {
                    bool silent = _currentThread.Context.Silent;
                    _currentThread.BreakExecution();

                    if (!_currentThread.HasProgramRunning())
                    {
                        _threadManager.Remove(_currentThread);
                        _interpreterThread.Start();
                        
                        if (!silent)
                        {
                            _shared.Screen.Print("Program ended.");
                        }
                    }
                }
            }
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

        private Variable GetOrCreateVariable(string identifier)
        {
            Variable variable;

            if (_vars.ContainsKey(identifier))
            {
                variable = GetVariable(identifier);
            }
            else
            {
                variable = new Variable();
                variable.Name = identifier;
                AddVariable(variable, identifier);
            }
            return variable;
        }

        private Variable GetVariable(string identifier)
        {
            identifier = identifier.ToLower();
            if (_vars.ContainsKey(identifier))
            {
                return _vars[identifier];
            }
            else
            {
                throw new Exception(string.Format("Variable {0} is not defined", identifier.TrimStart('$')));
            }
        }

        public void AddVariable(Variable variable, string identifier)
        {
            identifier = identifier.ToLower();
            
            if (!identifier.StartsWith("$"))
            {
                identifier = "$" + identifier;
            }

            if (_vars.ContainsKey(identifier))
            {
                _vars.Remove(identifier);
            }

            _vars.Add(identifier, variable);
        }

        public bool VariableIsRemovable(Variable variable)
        {
            return !(variable is Binding.BoundVariable);
        }

        public void RemoveVariable(string identifier)
        {
            identifier = identifier.ToLower();
            
            if (_vars.ContainsKey(identifier) &&
                VariableIsRemovable(_vars[identifier]))
            {
                _vars.Remove(identifier);
            }
        }

        public void RemoveAllVariables()
        {
            List<string> removals = new List<string>();
            
            foreach (KeyValuePair<string, Variable> kvp in _vars)
            {
                if (VariableIsRemovable(kvp.Value))
                {
                    removals.Add(kvp.Key);
                }
            }

            foreach (string identifier in removals)
            {
                _vars.Remove(identifier);
            }
        }

        public object GetValue(object testValue)
        {
            // $cos     cos named variable
            // cos()    cos trigonometric function
            // cos      string literal "cos"

            if (testValue is string &&
                ((string)testValue).StartsWith("$"))
            {
                // value is a variable
                string identifier = (string)testValue;
                Variable variable = GetVariable(identifier);
                return variable.Value;
            }
            else
            {
                return testValue;
            }
        }

        public void SetValue(string identifier, object value)
        {
            Variable variable = GetOrCreateVariable(identifier);
            variable.Value = value;
        }

        public object PopValue()
        {
            return GetValue(PopStack());
        }

        public void AddTrigger(int triggerFunctionPointer)
        {
            ProgramContext context = _currentThread.Context;
            if (!context.Triggers.Contains(triggerFunctionPointer))
            {
                context.Triggers.Add(triggerFunctionPointer);
            }
        }

        public void RemoveTrigger(int triggerFunctionPointer)
        {
            ProgramContext context = _currentThread.Context;
            if (context.Triggers.Contains(triggerFunctionPointer))
            {
                context.Triggers.Remove(triggerFunctionPointer);
            }
        }

        public void StartWait(double waitTime)
        {
            if (waitTime > 0)
            {
                _currentThread.TimeWaitUntil = _currentTime + waitTime;
            }
            _currentThread.Status = ProgramStatus.Waiting;
        }

        public void EndWait()
        {
            _currentThread.TimeWaitUntil = 0;
            _currentThread.Status = ProgramStatus.Running;
        }

        public void Update(double deltaTime)
        {
            _currentTime = _shared.UpdateHandler.CurrentTime;

            try
            {
                PreUpdateBindings();

                foreach (kThread thread in _threadManager.Threads)
                {
                    if (thread.Enabled && thread.Context != null)
                    {
                        _currentThread = thread;

                        ProcessTriggers();
                        ProcessWait();

                        if (thread.Status == ProgramStatus.Running)
                        {
                            ContinueExecution();
                        }
                    }
                }

                _currentThread = _threadManager.FindLastForegroundThread();

                PostUpdateBindings();
            }
            catch (Exception e)
            {
                if (_shared.Logger != null)
                {
                    _shared.Logger.Log(e, _currentThread.Context.InstructionPointer);
                }

                if (_currentThread == _interpreterThread)
                {
                    SkipCurrentInstructionId();
                }
                else
                {
                    // break execution of all programs in the current thread and restart interpreter thread
                    _threadManager.Remove(_currentThread);
                    _interpreterThread.Start();
                }
            }
        }

        private void PreUpdateBindings()
        {
            if (_shared.BindingMgr != null)
            {
                _shared.BindingMgr.PreUpdate();
            }
        }

        private void PostUpdateBindings()
        {
            if (_shared.BindingMgr != null)
            {
                _shared.BindingMgr.PostUpdate();
            }
        }

        private void ProcessWait()
        {
            if (_currentThread.Status == ProgramStatus.Waiting && _currentThread.TimeWaitUntil > 0)
            {
                if (_currentTime >= _currentThread.TimeWaitUntil)
                {
                    EndWait();
                }
            }
        }

        private void ProcessTriggers()
        {
            ProgramContext context = _currentThread.Context;
            if (context.Triggers.Count > 0)
            {
                int currentInstructionPointer = context.InstructionPointer;
                List<int> triggerList = new List<int>(context.Triggers);

                foreach (int triggerPointer in triggerList)
                {
                    context.InstructionPointer = triggerPointer;

                    bool executeNext = true;
                    while (executeNext)
                    {
                        executeNext = ExecuteInstruction(context);
                    }
                }

                context.InstructionPointer = currentInstructionPointer;
            }
        }

        private void ContinueExecution()
        {
            int instructionCounter = 0;
            bool executeNext = true;
            int instructionsPerUpdate = Config.GetInstance().InstructionsPerUpdate;
            
            while (_currentThread.Status == ProgramStatus.Running && 
                   instructionCounter < instructionsPerUpdate &&
                   executeNext &&
                   _currentThread.Context != null)
            {
                executeNext = ExecuteInstruction(_currentThread.Context);
                instructionCounter++;
            }
        }

        private bool ExecuteInstruction(ProgramContext context)
        {
            Opcode opcode = context.Program[context.InstructionPointer];
            if (!(opcode is OpcodeEOF || opcode is OpcodeEOP))
            {
                opcode.Execute(this);
                context.InstructionPointer += opcode.DeltaInstructionPointer;
                return true;
            }
            else
            {
                if (opcode is OpcodeEOP)
                {
                    BreakExecution(false);
                }
                return false;
            }
        }

        private void SkipCurrentInstructionId()
        {
            if (_currentThread == _interpreterThread)
            {
                ProgramContext context = _interpreterThread.Context;
                int currentInstructionId = context.Program[context.InstructionPointer].InstructionId;

                while (context.InstructionPointer < context.Program.Count &&
                       context.Program[context.InstructionPointer].InstructionId == currentInstructionId)
                {
                    context.InstructionPointer++;
                }
            }
        }

        public void CallBuiltinFunction(string functionName)
        {
            if (_functions.ContainsKey(functionName))
            {
                FunctionBase function = _functions[functionName];
                function.Execute(_shared);
            }
            else
            {
                throw new Exception("Call to non-existent function " + functionName);
            }
        }

        public void ToggleFlyByWire(string paramName, bool enabled)
        {
            if (_shared.BindingMgr != null)
            {
                _shared.BindingMgr.ToggleFlyByWire(paramName, enabled);
                _currentThread.Context.ToggleFlyByWire(paramName, enabled);
            }
        }

        public kThread CreateThread(string programName, object[] parameters)
        {
            return _threadManager.CreateThread(_currentThread, programName, parameters);
        }

        public void OnSave(ConfigNode node)
        {
            try
            {
                ConfigNode contextNode = new ConfigNode("context");

                // Save variables
                if (_vars.Count > 0)
                {
                    ConfigNode varNode = new ConfigNode("variables");

                    foreach (var kvp in _vars)
                    {
                        if (!(kvp.Value is Binding.BoundVariable) &&
                            (kvp.Value.Name.IndexOfAny(new char[] { '*', '-' }) == -1))  // variables that have this characters are internal and shouldn't be persisted
                        {
                            varNode.AddValue(kvp.Key.TrimStart('$'), Persistence.ProgramFile.EncodeLine(kvp.Value.Value.ToString()));
                        }
                    }

                    contextNode.AddNode(varNode);
                }

                node.AddNode(contextNode);
            }
            catch (Exception e)
            {
                if (_shared.Logger != null) _shared.Logger.Log(e);
            }
        }

        public void OnLoad(ConfigNode node)
        {
            try
            {
                StringBuilder scriptBuilder = new StringBuilder();

                foreach (ConfigNode contextNode in node.GetNodes("context"))
                {
                    foreach (ConfigNode varNode in contextNode.GetNodes("variables"))
                    {
                        foreach (ConfigNode.Value value in varNode.values)
                        {
                            string varValue = Persistence.ProgramFile.DecodeLine(value.value);
                            scriptBuilder.AppendLine(string.Format("set {0} to {1}.", value.name, varValue));
                        }
                    }
                }

                if (_shared.ScriptHandler != null && scriptBuilder.Length > 0)
                {
                    ProgramBuilder programBuilder = new ProgramBuilder();
                    programBuilder.AddRange(_shared.ScriptHandler.Compile(scriptBuilder.ToString()));
                    List<Opcode> program = programBuilder.BuildProgram(false);
                    RunProgram(program, true);
                }
            }
            catch (Exception e)
            {
                if (_shared.Logger != null) _shared.Logger.Log(e);
            }
        }
    }
}
