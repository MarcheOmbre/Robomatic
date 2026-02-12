#define SHOW_DEBUG_LOG

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Interpreters.Lua.Libraries;
using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;
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

        private static string FormatEnumName(string name) => name is not { Length: > 0 } ? string.Empty : name.ToUpper();


        private static void RegisterDynamicMembers(Dictionary<Type, HashSet<MethodInfo>> members)
        {
            members ??= new Dictionary<Type, HashSet<MethodInfo>>();

            foreach (var (type, methodInfos) in members)
            {
                if(!type.IsClass)
                    throw new ApplicationException($"Type {type.Name} must be a class.");
                
                if (methodInfos is not { Count: > 0 })
                    throw new ApplicationException($"No method found for type {type.Name}");

                if (type.IsAbstract && type.IsSealed)
                    throw new ApplicationException($"Type {type.Name} must be dynamic.");

                // Hide all members except authorized ones
                var description = new StandardUserDataDescriptor(type, InteropAccessMode.HideMembers);

                // Add back all authorized members
                foreach (var methodInfo in methodInfos)
                {
                    var name = FormatFunctionName(methodInfo.Name);
                    description.AddMember(name, new MethodMemberDescriptor(methodInfo));

#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering dynamic member : {type.Name}.{name}");
#endif
                }

                // Add the type to the script
                UserData.RegisterType(type, description);
            }
        }

        private static void RegisterStaticMembers(Script script, Dictionary<Type, HashSet<MethodInfo>> members)
        {
            members ??= new Dictionary<Type, HashSet<MethodInfo>>();

            var table = new Table(script);
            foreach (var (type, methodInfos) in members)
            {
                if(!type.IsClass)
                    throw new ApplicationException($"Type {type.Name} must be a class.");

                if (methodInfos is not { Count: > 0 })
                    throw new ApplicationException($"No method found for type {type.Name}");

                if (!type.IsAbstract || !type.IsSealed)
                    throw new ApplicationException($"Type {type.Name} must be static.");

                var typeName = type.Name;
                foreach (var methodInfo in methodInfos)
                {
                    var methodName = FormatFunctionName(methodInfo.Name);
                    table[methodName] = methodInfo;   
                    
#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering static member : {typeName}.{methodName}");
#endif
                }
                
                script.Globals[typeName] = table;
            }
        }

        private static void RegisterGlobalEnums(Script script, HashSet<Type> types)
        {
            types ??= new HashSet<Type>();
            
            foreach (var type in types)
            {
                if(!type.IsEnum)
                    throw new ApplicationException($"Type {type.Name} must be an enum.");
                
                var values = (int[])Enum.GetValues(type);
                var names = Enum.GetNames(type);

                if (values.Length != names.Length)
                    throw new ApplicationException("Enum values and names must have the same length.");

                for (var i = 0; i < values.Length; i++)
                {
                    var name = FormatEnumName($"{type.Name}_{names[i]}");
                    script.Globals[name] = values[i];
                    
#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering enum : {name}");
#endif
                }
            }
        }



        private static void RegisterExternalModules(Script script, HashSet<ALuaLibrary> modules)
        {
            if (modules is not { Count: > 0 })
                throw new ApplicationException("No modules to load.");

            foreach (var module in modules)
            {
                var methods = module.ExtractMethods();
                if (methods is not { Length: > 0 })
                    return;

                // Load each resource
                foreach (var method in methods)
                {
                    if (string.IsNullOrEmpty(method))
                        throw new ApplicationException("Module cannot be empty.");

                    script.DoString(method);

#if UNITY_EDITOR && SHOW_DEBUG_LOG
                    Debug.Log($"Registering module : {module.Name}");
#endif
                }
            }
        }
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


        public async Task Execute(Dictionary<Type, HashSet<MethodInfo>> members, string code,
            CancellationToken cancellationToken = default)
        {
            // Create the script and load the code
            var script = new Script(coreModules) { DebuggerEnabled = debuggerEnabled };

            // Load memners
            members ??= new Dictionary<Type, HashSet<MethodInfo>>();

            var dynamicTypes = new Dictionary<Type, HashSet<MethodInfo>>();
            var staticTypes = new Dictionary<Type, HashSet<MethodInfo>>();
            var enumTypes = new HashSet<Type>();

            foreach (var member in members)
            {
                if (member.Key.IsClass)
                {
                    if (!member.Key.IsAbstract || !member.Key.IsSealed)
                        dynamicTypes.Add(member.Key, member.Value);
                    else
                        staticTypes.Add(member.Key, member.Value);
                }
                else if (member.Key.IsEnum)
                    enumTypes.Add(member.Key);
            }

            RegisterDynamicMembers(dynamicTypes);
            RegisterStaticMembers(script, staticTypes);
            RegisterGlobalEnums(script, enumTypes);
            
            // Register modules
            RegisterExternalModules(script, new HashSet<ALuaLibrary>
            {
                new SystemLibrary()
            });

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