#define DEBUG_REGISTRATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Log;
using Project.Scripts.Interpreters.Lua.Libraries;
using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;

namespace Project.Scripts.Interpreters.Lua
{
    /// <summary>
    /// Service for executing Lua scripts.
    /// </summary>
    public class LuaInterpreterService : IInterpreterService
    {
        private struct ScriptData
        {
            public Script Script;
            public CancellationTokenSource CancellationTokenSource;
            public Task Task;
        }
        
        private const string SelfKeyword = "self";
        
        
        public event Action<IProgrammable> OnScriptAdded = delegate { };
        public event Action<IProgrammable> OnScriptRemoved =delegate { };
        
        
        private readonly CoreModules coreModules;
        private readonly Logger logger;

        private readonly Dictionary<Type, StandardUserDataDescriptor> dynamicMembers;
        private readonly Dictionary<Type, Dictionary<MethodInfo, string>> staticMembers;
        private readonly Dictionary<string, string> enums;
        private readonly HashSet<string> luaStaticClasses;
        private readonly HashSet<ALuaObject> luaObjects = new(){ new Vector2Object() };
        
        private readonly Dictionary<IProgrammable, ScriptData> runningScripts = new();


        public LuaInterpreterService(CoreModules coreModules, Logger logger = null)
        {
            this.coreModules = coreModules;
            this.logger = logger;
            
            var members = AuthorizedHelper.ExtractTypesAndMethods(GetType().Assembly);
            
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

            dynamicMembers = EnvironmentUtils.ExtractDynamicMembers(dynamicTypes);
            staticMembers = EnvironmentUtils.ExtractStaticMembers(staticTypes);
            enums = EnvironmentUtils.ExtractGlobalEnums(enumTypes);
            luaStaticClasses = EnvironmentUtils.ExtractLuaStaticClass(new HashSet<ALuaStaticClass>
            {
                new SystemStaticClass()
            });
        }

        ~LuaInterpreterService()
        {
            foreach (var key in runningScripts.Keys)
                Remove(key);
        }

        
        private async Task ScriptProcess(IProgrammable reference, string code, Script script, CancellationToken cancellationToken)
        {
            try
            {
                code = Utils.FormatCode(code);
                var function = script.DoString(code);

                var coroutine = script.CreateCoroutine(function);
                foreach (DynValue unused in coroutine.Coroutine.AsEnumerable())
                {
                    await Task.Yield();

                    // Handle coroutine cancellation
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (reference is null)
                        break;
                }
            }
            catch (Exception e)
            {
                var line = Utils.ExtractLine(e);

                if (reference != null)
                    logger?.AddLog(reference, LogType.Error, e.Message, line);
            }
            finally
            {
                Remove(reference);
            }
        }
        
        public HashSet<IProgrammable> GetInstances() => runningScripts.Keys.ToHashSet();
        
        public bool Inject(RuntimeEnvironment runtimeEnvironment)
        {
            if(string.IsNullOrEmpty(runtimeEnvironment.Code))
                return false;
            
            if(runtimeEnvironment.Reference is null)
                throw new ArgumentNullException(nameof(runtimeEnvironment.Reference));
            
            // Remove previous script if any
            Remove(runtimeEnvironment.Reference);
            
            // Create the script and load the code
            var script = new Script(coreModules);

            // Inject Dynamic members
            foreach (var (type, description) in dynamicMembers) 
                UserData.RegisterType(type, description);
            
            // Inject Static members
            foreach (var (type, methodsDictionary) in staticMembers)
            {
                var typeName = type.Name;
                var table = new Table(script);
                
                foreach (var (methodInfo, name) in methodsDictionary) 
                    table[name] = methodInfo;
                
                script.Globals[typeName] = table;
            }
            
            // Inject Enums
            foreach (var (name, value) in enums) 
                script.Globals[name] = value;
            
            // Inject Static classes
            foreach (var luaStaticClass in luaStaticClasses) 
                script.DoString(luaStaticClass);
            
            // Inject Objects
            foreach (var luaObject in luaObjects) 
                luaObject.Register(script);
            
            // Inject self
            script.Globals[SelfKeyword] = runtimeEnvironment.Reference;
            
            // Create the script process
            var cancellationTokenSource = new CancellationTokenSource();
            var task = ScriptProcess(runtimeEnvironment.Reference, runtimeEnvironment.Code, script, cancellationTokenSource.Token);
            var scriptData = new ScriptData { Script = script, CancellationTokenSource = cancellationTokenSource, Task = task };
            
            runningScripts.Add(runtimeEnvironment.Reference, scriptData);
            
            logger?.AddLog(runtimeEnvironment.Reference, LogType.Information, "Script injected");
            OnScriptAdded(runtimeEnvironment.Reference);
            return true;
        }

        public bool Remove(IProgrammable reference)
        {
            if(!runningScripts.TryGetValue(reference, out var scriptData))
                return false;

            // Cancel the script process
            if (scriptData.CancellationTokenSource != null)
            {
                scriptData.CancellationTokenSource.Cancel();
                scriptData.CancellationTokenSource.Dispose();
            }

            runningScripts.Remove(reference);
            
            logger?.AddLog(reference, LogType.Information, "Script removed");
            OnScriptRemoved(reference);
            return true;
        }
    }
}