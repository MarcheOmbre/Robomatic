#define DEBUG

using System;
using System.Collections.Generic;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua
{
    public static class EnvironmentUtils
    {
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

        private static string FormatEnumName(string name) =>
            name is not { Length: > 0 } ? string.Empty : name.ToUpper();


        public static Dictionary<Type, StandardUserDataDescriptor> ExtractDynamicMembers(
            Dictionary<Type, HashSet<MethodInfo>> members)
        {
            members ??= new Dictionary<Type, HashSet<MethodInfo>>();

            var result = new Dictionary<Type, StandardUserDataDescriptor>();

            foreach (var (type, methodInfos) in members)
            {
                var typeName = type.Name;
                
                if (!type.IsClass)
                    throw new ApplicationException($"Type {typeName} must be a class.");

                if (methodInfos is not { Count: > 0 })
                    throw new ApplicationException($"No method found for type {typeName}");

                if (type.IsAbstract && type.IsSealed)
                    throw new ApplicationException($"Type {typeName} must be dynamic.");

                // Hide all members except authorized ones
                var description = new StandardUserDataDescriptor(type, InteropAccessMode.HideMembers);

                // Inject back all authorized members
                foreach (var methodInfo in methodInfos)
                {
                    var name = FormatFunctionName(methodInfo.Name);
                    description.AddMember(name, new MethodMemberDescriptor(methodInfo));

#if UNITY_EDITOR && DEBUG
                    Debug.Log($"Extracting dynamic member : {typeName}.{name}");
#endif
                }

                // Inject the type to the script
                result.Add(type, description);
            }

            return result;
        }

        public static Dictionary<Type, Dictionary<MethodInfo, string>> ExtractStaticMembers(Dictionary<Type, HashSet<MethodInfo>> members)
        {
            members ??= new Dictionary<Type, HashSet<MethodInfo>>();

            var result = new Dictionary<Type, Dictionary<MethodInfo, string>>();

            foreach (var (type, methodInfos) in members)
            {
                var typeName = type.Name;
                
                if (!type.IsClass)
                    throw new ApplicationException($"Type {typeName} must be a class.");

                if (methodInfos is not { Count: > 0 })
                    throw new ApplicationException($"No method found for type {typeName}");

                if (!type.IsAbstract || !type.IsSealed)
                    throw new ApplicationException($"Type {typeName} must be static.");
                
                var methodsInfo = new Dictionary<MethodInfo, string>();
                
                foreach (var methodInfo in methodInfos)
                {
                    var methodName = FormatFunctionName(methodInfo.Name);
                    
                    if (!methodsInfo.TryAdd(methodInfo, methodName))
                        throw new ApplicationException($"Method {methodInfo.Name} already registered.");

#if UNITY_EDITOR && DEBUG
                    Debug.Log($"Extracting static member : {typeName}.{methodName}");
#endif
                }
                
                if(!result.TryAdd(type, methodsInfo))
                    throw new ApplicationException($"Type {typeName} already registered.");
            }

            return result;
        }

        public static Dictionary<string, int> ExtractGlobalEnums(HashSet<Type> types)
        {
            types ??= new HashSet<Type>();

            var result = new Dictionary<string, int>();

            foreach (var type in types)
            {
                var typeName = type.Name;
                
                if (!type.IsEnum)
                    throw new ApplicationException($"Type {typeName} must be an enum.");

                var values = (int[])Enum.GetValues(type);
                var names = Enum.GetNames(type);

                if (values.Length != names.Length)
                    throw new ApplicationException("Enum values and names must have the same length.");

                for (var i = 0; i < values.Length; i++)
                {
                    var name = FormatEnumName($"{typeName}_{names[i]}");

                    if (!result.TryAdd(name, values[i]))
                        throw new ApplicationException($"Enum {name} already registered.");

#if UNITY_EDITOR && DEBUG
                    Debug.Log($"Extracting enum : {name}");
#endif
                }
            }

            return result;
        }
        
        public static HashSet<string> ExtractLuaStaticClass(HashSet<ALuaStaticClass> luaStaticClasses)
        {
            if (luaStaticClasses is not { Count: > 0 })
                throw new ApplicationException("No Lua Static Class to load.");

            var result = new HashSet<string>();
            
            foreach (var module in luaStaticClasses)
            {
                var methods = module.ExtractMethods();
                if (methods is not { Length: > 0 })
                    continue;

                // Load each resource
                foreach (var method in methods)
                {
                    if (string.IsNullOrEmpty(method))
                        throw new ApplicationException("Lua Static Class cannot be empty.");

                    result.Add(method);

#if UNITY_EDITOR && DEBUG
                    Debug.Log($"Extracting Lua Static Classes : {module.Name}");
#endif
                }
            }
            
            return result;
        }
    }
}