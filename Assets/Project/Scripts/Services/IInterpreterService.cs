using System;
using System.Collections.Generic;
using System.Reflection;

namespace Project.Scripts.Services
{
    public interface IInterpreterService
    {
        public void LoadScript(IEnumerable<MethodInfo> globalMethodInfos, Dictionary<Type, IEnumerable<MethodInfo>> methodInfos, string code);
        
        public void CallFunction(string functionName);
        
        public void Clear();
    }
}