using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Interpreters.Abstracts;
using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Interpreters
{
    [Serializable]
    [CreateAssetMenu(fileName = "LuaInterpreterSettings", menuName = "Interpreter/Lua Interpreter Settings", order = 0)]
    public class LuaInterpreterSettings : AInterpreterSettings
    {
        [SerializeField] private CoreModules coreModules;
        [SerializeField] private bool debuggerEnabled;


        protected override IInterpreterService GetInterpreterService()
        {
            return new LuaInterpreterService(coreModules, debuggerEnabled);
        }
    }
    
    
    /// <summary>
    /// Service for executing Lua scripts.
    /// </summary>
    public class LuaInterpreterService : IInterpreterService
    {
        private readonly CoreModules coreModules;
        private readonly bool debuggerEnabled;

        private Script currentScript;
        

        public LuaInterpreterService(CoreModules coreModules, bool debuggerEnabled)
        {
            this.coreModules = coreModules;
            this.debuggerEnabled = debuggerEnabled;
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
            
            foreach (var (type, typeDelegates) in localMethodInfos)
            {
                if (type.IsEnum)
                    continue;
                
                var description = (StandardUserDataDescriptor)UserData.RegisterType(type);
                var members = description.Members.ToArray();
                    
                // Remove all members
                foreach (var member in members) 
                    description.RemoveMember(member.Key);
                
                if(typeDelegates is null)
                    continue;
                
                // Add authorized members
                var allowedMemberNames = typeDelegates.Select(method => method.Name).ToArray();
                foreach (var authorizedMember in members.Where(m => allowedMemberNames.Contains(m.Key)))
                {
                    authorizedMember.Deconstruct(out var key, out var value);
                    description.AddMember(FormatFunctionName(key), value);
                }
            }
        }

        private static void RegisterGlobalEnums(Script script, IEnumerable<Type> types)
        {
            types ??= Array.Empty<Type>();

            foreach (var type in types)
            {
                UserData.RegisterType(type);
                
                var values = (int[])Enum.GetValues(type);
                var names = Enum.GetNames(type);
                
                if(values.Length != names.Length)
                    throw new InvalidOperationException("Enum values and names must have the same length.");

                for (var i = 0; i < values.Length; i++) 
                    script.Globals[$"{type.Name}_{names[i]}"] = values[i];
            }
        }
        
        
        public void LoadScript(IEnumerable<MethodInfo> globalMethodInfos, Dictionary<Type, IEnumerable<MethodInfo>> methodInfos, string code)
        {
            // Clear references and functions
            Clear();

            // Create the script and load the code
            currentScript = new Script(coreModules) { DebuggerEnabled = debuggerEnabled };

            // Load globals delegates
            RegisterGlobalMethodInfos(currentScript, globalMethodInfos);
       
            
            // Load delegates per types
            methodInfos ??= new Dictionary<Type, IEnumerable<MethodInfo>>();
            RegisterTypes(methodInfos.Where(x => !x.Key.IsEnum)
                .ToDictionary(x => x.Key, x => x.Value));
            
            // Load enums
            RegisterGlobalEnums(currentScript, methodInfos.Keys.Where(x => x.IsEnum));
            
            
            // Load script
            currentScript.DoString(code);
        }

        public void CallFunction(string functionName)
        {
            if(currentScript is null)
                throw new InvalidOperationException("Script is not loaded.");

            var function = currentScript.Globals.Get(functionName);
            if (function is null || function.IsNil())
                return;
            
            currentScript.Call(function);
        }

        public void Clear()
        {
            currentScript = null;
        }
    }
}