#define SHOW_DEBUG_LOG

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
        private readonly TextAsset[] externalModules;
        private readonly bool debuggerEnabled;


        public LuaInterpreterService(CoreModules coreModules, TextAsset[] modules, bool debuggerEnabled)
        {
            this.coreModules = coreModules;
            this.debuggerEnabled = debuggerEnabled;
            externalModules = modules ?? Array.Empty<TextAsset>();

            // Register custom Types
            new CustomVector2().Register();
            new CustomVector3().Register();
        }


        private static string FormatFunctionName(string functionName)
        {
            if (functionName is not { Length: > 0 })
                return string.Empty;

            for (var i = functionName.Length - 1; i >= 1; i--)
            {
                if (!char.IsUpper(functionName[i]))
                    continue;

                if (functionName[i - 1] != '_')
                    functionName = functionName.Insert(i, "_");
            }

            return functionName.ToLower();
        }

        private static string FormatEnumName(string name)
        {
            if (name is not { Length: > 0 })
                return string.Empty;

            for (var i = name.Length - 1; i >= 1; i--)
            {
                if (!char.IsUpper(name[i]))
                    continue;

                if (name[i - 1] != '_')
                    name = name.Insert(i, "_");
            }

            return name.ToUpper();
        }

        private static void RegisterGlobalMethodInfos(Script script, HashSet<MethodInfo> globalMethodInfos)
        {
            globalMethodInfos ??= new HashSet<MethodInfo>();
            foreach (var globalMethodInfo in globalMethodInfos)
            {
                var name = FormatFunctionName(globalMethodInfo.Name);
                script.Globals[name] = globalMethodInfo;
                
#if UNITY_EDITOR && SHOW_DEBUG_LOG
                Debug.Log($"Registering global method : {name}");
#endif
            }
        }

        private static void RegisterMembers(Dictionary<Type, HashSet<MethodInfo>> localMethodInfos)
        {
            localMethodInfos ??= new Dictionary<Type, HashSet<MethodInfo>>();

            foreach (var (type, methodsInfos) in localMethodInfos)
            {
                if(methodsInfos is null)
                    continue;
                
                if (type.IsEnum)
                    continue;

                // Hide all members except authorized ones
                var description = new StandardUserDataDescriptor(type, InteropAccessMode.HideMembers);

                // Add back all authorized members
                foreach (var methodInfo in methodsInfos)
                {
                    var name = FormatFunctionName(methodInfo.Name);
                    description.AddMember(name, new MethodMemberDescriptor(methodInfo));

#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering member : {type.Name}.{name}");
#endif
                }

                // Add the type to the script
                UserData.RegisterType(type, description);


            }
        }

        private static void RegisterGlobalEnums(Script script, HashSet<Type> types)
        {
            types ??= new HashSet<Type>();

            foreach (var type in types)
            {
                var values = (int[])Enum.GetValues(type);
                var names = Enum.GetNames(type);

                if (values.Length != names.Length)
                    throw new InvalidOperationException("Enum values and names must have the same length.");

                for (var i = 0; i < values.Length; i++)
                {
                    script.Globals[$"{type.Name}_{names[i]}"] = values[i];
#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering enum : {type.Name}_{names[i]}");
#endif
                }
            }
        }

        /*
        private static void RegisterExternalModules(Script script, TextAsset[] modules)
        {
            if (modules is not { Length: > 0 })
                return;

            // Load each resource
            foreach (var module in modules)
            {
                var moduleScript = new Script();
                var function = moduleScript.DoString(module.text);
                
                var moduleName = FormatFunctionName(module.name);
                script.Registry[moduleName] = function;
         
#if UNITY_EDITOR && SHOW_DEBUG_LOG
                Debug.Log($"Registering module : {moduleName}");
#endif
            }
        }
        */

        private static string PostProcess(string code)
        {
            if (code is null)
                return string.Empty;

            // Add yield at each loop
            code = code.Replace("do", "do coroutine.yield()");

            // Wrap the code in a function
            code = $" return function() {code} end";

            return code;
        }


        public async Task Execute(HashSet<MethodInfo> globalMethodInfos,
            Dictionary<Type, HashSet<MethodInfo>> methodInfos, string code,
            CancellationToken cancellationToken = default)
        {
            // Create the script and load the code
            var script = new Script(coreModules) { DebuggerEnabled = debuggerEnabled };

            // Load globals delegates
            RegisterGlobalMethodInfos(script, globalMethodInfos);

            // Load delegates per types
            methodInfos ??= new Dictionary<Type, HashSet<MethodInfo>>();
            RegisterMembers(methodInfos.Where(x => !x.Key.IsEnum)
                .ToDictionary(x => x.Key, x => x.Value));

            // Load enums
            RegisterGlobalEnums(script, methodInfos.Keys.Where(x => x.IsEnum).ToHashSet());

            // Add Lua external functions
            //RegisterExternalModules(script, externalModules);

            // Encapsulate in coroutine
            code = PostProcess(code);

            // Execute the code
            var function = script.DoString(code);

            if (function.Table != null)
            {
                foreach (var table in function.Table.Pairs)
                {
                    Debug.Log($"{table.Key} - {table.Value}");
                }
            }

            var coroutine = script.CreateCoroutine(function);
            foreach (DynValue unused in coroutine.Coroutine.AsEnumerable())
            {
                await Task.Yield();

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        public string FormatErrorMessage(Exception exception)
        {
            if (exception is null)
                return string.Empty;

            if (exception is SyntaxErrorException syntaxErrorException)
            {
                var decoratedMessage = syntaxErrorException.DecoratedMessage;

                var firstIndex = decoratedMessage.IndexOf('(');
                var lastIndex = decoratedMessage.LastIndexOf(',');

                if (firstIndex != -1 && lastIndex != -1)
                {
                    var line = decoratedMessage.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
                    return $"Error at line {line}: {syntaxErrorException.Message}";
                }
            }

            return exception.Message;
        }
    }
}