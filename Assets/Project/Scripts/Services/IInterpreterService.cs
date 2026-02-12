using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Scripts.Services
{
    public interface IInterpreterService
    {
        public Task Execute(HashSet<MethodInfo> globalMethodInfos, Dictionary<Type, HashSet<MethodInfo>> methodInfos, string code,
            CancellationToken cancellationToken = default);
        
        public string FormatErrorMessage(Exception exception);
    }
}