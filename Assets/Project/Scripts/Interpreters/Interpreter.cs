using System;
using System.Collections.Generic;
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
            var methodInfosPerType = AuthorizedHelper.ExtractTypesAndMethods();
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
                if (e is not OperationCanceledException)
                    Debug.LogException(e);
            }

            interpreterSettings.Service.Clear();
            cancellationTokenSource = null;
        }

        public void Stop() => cancellationTokenSource?.Cancel();
    }
}