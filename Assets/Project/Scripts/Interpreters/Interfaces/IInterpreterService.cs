using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Scripts.Interpreters.Interfaces
{
    public interface IInterpreterService
    {
        public Task Execute(Dictionary<Type, HashSet<MethodInfo>> members, string code,
            CancellationToken cancellationToken = default);
        
        public string FormatErrorMessage(Exception exception);
    }
}