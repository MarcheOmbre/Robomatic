using System;
using System.Collections.Generic;
using System.Linq;
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
        
        
        public void LoadScript(IEnumerable<Delegate> globalsDelegates, Dictionary<Type, IEnumerable<Delegate>> delegatesPerTypes, string code)
        {
            // Clear references and functions
            Clear();

            // Create the script and load the code
            currentScript = new Script(coreModules) { DebuggerEnabled = debuggerEnabled };

            // Load globals delegates
            foreach (var globalsDelegate in globalsDelegates) 
                currentScript.Globals[FormatFunctionName(globalsDelegate.Method.Name)] = globalsDelegate;
            
            // Load delegates per types
            if (delegatesPerTypes is not null)
            {
                foreach (var (type, typeDelegates) in delegatesPerTypes)
                {
                    var description = (StandardUserDataDescriptor)UserData.RegisterType(type);
                    var allowedMemberNames = typeDelegates.Select(d => d.Method.Name).ToArray();
                    
                    var members = description.Members.ToArray();
                    
                    // Remove all members
                    foreach (var member in members) 
                        description.RemoveMember(member.Key);
                    
                    // Add authorized members
                    foreach (var authorizedMember in members.Where(m => allowedMemberNames.Contains(m.Key)))
                    {
                        authorizedMember.Deconstruct(out var key, out var value);
                        description.AddMember(FormatFunctionName(key), value);
                    }
                }
            }
            
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