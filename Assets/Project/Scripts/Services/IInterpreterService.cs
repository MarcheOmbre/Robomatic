using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Scripts.Services
{
    public interface IInterpreterService
    {
        public Task Execute(IEnumerable<MethodInfo> globalMethodInfos, Dictionary<Type, IEnumerable<MethodInfo>> methodInfos, string code,
            CancellationToken cancellationToken = default);
    }
}