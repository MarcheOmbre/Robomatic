using System;
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
        [SerializeField] private AInterpreterSettings interpreterSettings;


        private CancellationTokenSource cancellationTokenSource;


        private void OnDisable() => Stop();


        public async Task Run(string code)
        {
            if (cancellationTokenSource is not null)
                throw new InvalidOperationException("Script is already running.");

            cancellationTokenSource = new CancellationTokenSource();

            // Load delegates
            var methodInfosPerType = AuthorizedHelper.ExtractTypesAndMethods();

            // Run the interpreter
            try
            {
                await interpreterSettings.Service.Execute(methodInfosPerType, code,
                    cancellationTokenSource.Token);
            }
            catch (Exception error)
            {
                Debug.LogWarning(interpreterSettings.Service.FormatErrorMessage(error));
            }
            finally
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        public void Stop() => cancellationTokenSource?.Cancel();
    }
}