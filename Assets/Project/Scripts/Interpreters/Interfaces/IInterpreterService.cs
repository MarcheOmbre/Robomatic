using System;
using System.Collections.Generic;

namespace Project.Scripts.Interpreters.Interfaces
{
    public interface IInterpreterService
    {
        public event Action<IProgrammable> OnScriptAdded;
        public event Action<IProgrammable> OnScriptRemoved;
        
        public HashSet<IProgrammable> GetInstances();
        
        public bool Inject(RuntimeEnvironment runtimeEnvironment);
        
        public bool Remove(IProgrammable reference);
    }
}