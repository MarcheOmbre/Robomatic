using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua
{
    /// <summary>
    /// Service for executing Lua scripts.
    /// </summary>
    public class LuaInterpreterService : IInterpreterService
    {
        private readonly CoreModules coreModules;
        private readonly bool debuggerEnabled;
        private bool isCodePaused;


        public LuaInterpreterService(CoreModules coreModules, bool debuggerEnabled)
        {
            this.coreModules = coreModules;
            this.debuggerEnabled = debuggerEnabled;

            // Register custom Types
            new CustomVector2().Register();
            new CustomVector3().Register();
        }


        private static string FormatFunctionName(string functionName)
        {
            return functionName.Replace("get_", "Get")
                .Replace("set_", "Set");
        }

        private static void RegisterGlobalMethodInfos(Script script, IEnumerable<MethodInfo> globalMethodInfos)
        {
            globalMethodInfos ??= Array.Empty<MethodInfo>();
            foreach (var globalMethodInfo in globalMethodInfos)
                script.Globals[FormatFunctionName(globalMethodInfo.Name)] = globalMethodInfo;
        }

        private static void RegisterTypes(Dictionary<Type, IEnumerable<MethodInfo>> localMethodInfos)
        {
            localMethodInfos ??= new Dictionary<Type, IEnumerable<MethodInfo>>();

            foreach (var (type, methodsInfos) in localMethodInfos)
            {
                var methodsArray = methodsInfos?.ToArray() ?? Array.Empty<MethodInfo>();
                if (type.IsEnum)
                    continue;

                // Hide all members except authorized ones
                var description = new StandardUserDataDescriptor(type, InteropAccessMode.HideMembers);

                // Add back all authorized members
                foreach (var methodInfo in methodsArray)
                    description.AddMember(FormatFunctionName(methodInfo.Name), new MethodMemberDescriptor(methodInfo));

                // Add the type to the script
                UserData.RegisterType(type, description);

#if UNITY_EDITOR
                foreach (var descriptionMemberName in description.MemberNames)
                    Debug.Log($"Registering {type.Name}.{descriptionMemberName}");
#endif
            }
        }

        private static void RegisterGlobalEnums(Script script, IEnumerable<Type> types)
        {
            types ??= Array.Empty<Type>();

            foreach (var type in types)
            {
                var values = (int[])Enum.GetValues(type);
                var names = Enum.GetNames(type);

                if (values.Length != names.Length)
                    throw new InvalidOperationException("Enum values and names must have the same length.");

                for (var i = 0; i < values.Length; i++)
                {
                    script.Globals[$"{type.Name}_{names[i]}"] = values[i];
#if UNITY_EDITOR
                    Debug.Log($"Registering enum : {type.Name}_{names[i]}");
#endif
                }
            }
        }

        private static string PostProcess(string code)
        {
            // Add yield at each loop
            code = code.Replace("do", "do coroutine.yield()");
            
            // Wrap the code in a function
            code = $@" return function()
                        {code}
                    end";

            return code;
        }


        public async Task Execute(IEnumerable<MethodInfo> globalMethodInfos, Dictionary<Type, IEnumerable<MethodInfo>> methodInfos, string code, 
            CancellationToken cancellationToken = default)
        {
            // Create the script and load the code
            var script = new Script(coreModules) { DebuggerEnabled = debuggerEnabled };
            isCodePaused = false;
            
            // Load globals delegates
            RegisterGlobalMethodInfos(script, globalMethodInfos);

            // Load delegates per types
            methodInfos ??= new Dictionary<Type, IEnumerable<MethodInfo>>();
            RegisterTypes(methodInfos.Where(x => !x.Key.IsEnum)
                .ToDictionary(x => x.Key, x => x.Value));

            // Load enums
            RegisterGlobalEnums(script, methodInfos.Keys.Where(x => x.IsEnum));
            
            // Encapsulate in coroutine
            code = PostProcess(code);
            
            // Execute the code
            var function = script.DoString(code);
            var coroutine = script.CreateCoroutine(function);
            foreach (DynValue unused in coroutine.Coroutine.AsEnumerable())
            {
                // If paused, loop on the waiting state
                do
                {
                    await Task.Yield();
                } while (isCodePaused);
                
                if(cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}