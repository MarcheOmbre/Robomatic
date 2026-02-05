using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Project.Scripts.Interpreters.Abstracts;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Interpreters
{
    /// <summary>
    /// Base class for all interpreters.
    /// I do not add the prefix A for convenience with MonoBehaviorSingleton.
    /// </summary>
    public class Interpreter : MonoBehaviourSingleton<Interpreter>
    {
        private const string StartFunctionName = "Start";
        private const string UpdateFunctionName = "Update";
        private const string StopFunctionName = "Stop";

        [SerializeField] private AInterpreterSettings interpreterSettings;
        
        
        private CancellationTokenSource cancellationTokenSource;


        private void OnDisable() => Stop();


        private static Dictionary<Type, IEnumerable<MethodInfo>> ExtractMethodInfos()
        {
            var assembly = typeof(Context).Assembly;
            
            // Get authorized types
            var dictionary = assembly.GetTypes().Where(type => type.CustomAttributes
                .Any(attribute => attribute.AttributeType == typeof(AuthorizedType)))
                .ToDictionary<Type, Type, IEnumerable<MethodInfo>>(type => type, _ => Array.Empty<MethodInfo>());

            // Get authorized methods
            foreach (var type in assembly.GetTypes())
            {
                var delegates = type.GetMethods().Where(method => method.CustomAttributes
                    .Any(attribute => attribute.AttributeType == typeof(AuthorizedMethod)))
                    .ToArray();
                
                if(!delegates.Any())
                    continue;
                
                if(!dictionary.ContainsKey(type))
                    dictionary.Add(type, Array.Empty<MethodInfo>());
                
                dictionary[type] = dictionary[type].Concat(delegates);
            }
            
            return dictionary.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
        }
        
        
        private async Task GameLoopProcess(AInterpreterSettings interpreter, CancellationToken token)
        {
            interpreter.Service.CallFunction(StartFunctionName);
            
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                interpreter.Service.CallFunction(UpdateFunctionName);
                await Awaitable.EndOfFrameAsync(token);
            }

            interpreter.Service.CallFunction(StopFunctionName);
        }

        public async Task Run(string code)
        {
            if (cancellationTokenSource is not null)
                throw new InvalidOperationException("Script is already running.");

            // Load delegates
            var methodInfosPerType = ExtractMethodInfos();
            var globalMethodInfos = new List<MethodInfo>();
            
            // Separate context delegates from other delegates
            var contextType = typeof(Context);
            if (methodInfosPerType.ContainsKey(contextType))
            {
                globalMethodInfos.AddRange(methodInfosPerType[contextType]);
                methodInfosPerType.Remove(contextType);
            }
                
            interpreterSettings.Service.LoadScript(globalMethodInfos, methodInfosPerType, code);

            // Start the game loop
            cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                await GameLoopProcess(interpreterSettings, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                if(e is not OperationCanceledException)
                    Debug.LogException(e);
            }

            interpreterSettings.Service.Clear();
            cancellationTokenSource = null;
        }

        public void Stop() => cancellationTokenSource?.Cancel();
    }
}