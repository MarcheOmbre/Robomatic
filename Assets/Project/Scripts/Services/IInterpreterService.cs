using System;
using System.Collections.Generic;

namespace Project.Scripts.Services
{
    public interface IInterpreterService
    {
        public void LoadScript(IEnumerable<Delegate> globalsDelegates, Dictionary<Type, IEnumerable<Delegate>> delegatesPerTypes, string code);
        
        public void CallFunction(string functionName);
        
        public void Clear();
    }
}