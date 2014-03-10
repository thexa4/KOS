using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.Execution
{
    public class ContextManager
    {
        private SharedObjects _shared;
        private List<ProgramContext> _contexts;
        public ProgramContext CurrentContext { get; private set; }
        
        public int Count { get { return _contexts.Count; } }

        public ContextManager(SharedObjects shared)
        {
            _shared = shared;
            _contexts = new List<ProgramContext>();
            CurrentContext = null;
        }

        public void PushContext(ProgramContext context)
        {
            _contexts.Add(context);
            CurrentContext = context;
        }

        public void PopContext()
        {
            if (_contexts.Count > 0)
            {
                ProgramContext contextRemove = _contexts[_contexts.Count - 1];
                _contexts.Remove(contextRemove);
                contextRemove.OnContextPop(_shared);
                CurrentContext = (_contexts.Count > 0) ? _contexts[_contexts.Count - 1] : null;
            }
        }
    }
}
