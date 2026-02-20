#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Lua.Libraries;
using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;
using UnityEngine;
using Logger = Project.Scripts.Interpreters.Log.Logger;
using LogType = Project.Scripts.Interpreters.Log.LogType;

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
        public event Action<IProgrammable> OnScriptRemoved = delegate { };


        private readonly CoreModules coreModules;
        private readonly Logger logger;

        private readonly Dictionary<Type, StandardUserDataDescriptor> publicDynamicMethods;
        private readonly Dictionary<Type, Dictionary<MethodInfo, string>> publicStaticMembers;
        private readonly Dictionary<string, int> enums;
        private readonly Dictionary<string, List<string>> luaStaticClasses;
        private readonly HashSet<ALuaObject> luaObjects;

        private readonly Dictionary<IProgrammable, ScriptData> runningScripts = new();


        public LuaInterpreterService(CoreModules coreModules, Logger logger = null)
        {
            this.coreModules = coreModules;
            this.logger = logger;

            // Public Methods
            var dynamicTypes = new Dictionary<Type, HashSet<MethodInfo>>();
            var staticTypes = new Dictionary<Type, HashSet<MethodInfo>>();
            foreach (var member in AuthorizedHelper.ExtractPublicMethods(new[] { Assembly.GetExecutingAssembly() }))
            {
                if (!member.Key.IsAbstract || !member.Key.IsSealed)
                    dynamicTypes.Add(member.Key, member.Value);
                else
                    staticTypes.Add(member.Key, member.Value);
            }

            publicDynamicMethods = EnvironmentUtils.FormatDynamicMembers(dynamicTypes);
            publicStaticMembers = EnvironmentUtils.FormatStaticMembers(staticTypes);

            // Lua Classes
            luaStaticClasses = EnvironmentUtils.FormatLuaStaticClass(new HashSet<ALuaStaticClass>
            {
                new ConsoleStaticClass(),
                new SystemStaticClass()
            });
            
            // Lua Objects
            luaObjects = new HashSet<ALuaObject>
            {
                new Vector2Object()
            };

            // Enums
            enums = EnvironmentUtils.FormatGlobalEnums(AuthorizedHelper
                .ExtractTypes(new[] { Assembly.GetExecutingAssembly() })
                .Where(extractType => extractType.IsEnum).ToHashSet());
            
            
#if DEBUG
            var stringBuilder = new StringBuilder();
            
            // Dynamic methods
            stringBuilder.Append("Extracted dynamic methods");
            foreach (var publicDynamicMethod in publicDynamicMethods)
            {
                stringBuilder.Append(string.Format("{0}\t" + publicDynamicMethod.Key, Environment.NewLine));
                foreach (var member in publicDynamicMethod.Value.Members) 
                    stringBuilder.Append(string.Format("{0}\t\t" + member.Key, Environment.NewLine));
            }
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Static methods
            stringBuilder.Append("Extracted static methods");
            foreach (var publicStaticMember in publicStaticMembers)
            {
                stringBuilder.Append(string.Format("{0}\t" + publicStaticMember.Key, Environment.NewLine));
                foreach (var methodInfoTuple in publicStaticMember.Value) 
                    stringBuilder.Append(string.Format("{0}\t\t" + methodInfoTuple.Value, Environment.NewLine));
            }
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Lua classes
            stringBuilder.Append("Extracted Lua static classes");
            foreach (var luaStaticClass in luaStaticClasses) 
                stringBuilder.Append(string.Format("{0}\t" + luaStaticClass.Key, Environment.NewLine));
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();

            // Lua objects
            stringBuilder.Append("Extracted Lua objects");
            foreach (var luaObject in luaObjects) 
                stringBuilder.Append(string.Format("{0}\t" + luaObject.Name, Environment.NewLine));
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Enums
            stringBuilder.Append("Extracted enums");
            foreach (var enumTuple in enums)
                stringBuilder.Append($"{Environment.NewLine}\t{enumTuple.Key} = {enumTuple.Value}");
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();
#endif
        }

        ~LuaInterpreterService()
        {
            foreach (var key in runningScripts.Keys)
                InternalRemove(key);
        }


        private async Task ScriptProcess(IProgrammable reference, string code, Script script,
            CancellationToken cancellationToken)
        {
            try
            {
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
                InternalRemove(reference);
            }
        }

        private bool InternalRemove(IProgrammable reference)
        {
            if (!runningScripts.TryGetValue(reference, out var scriptData))
                return false;

            // Cancel the script process
            if (scriptData.CancellationTokenSource != null)
            {
                scriptData.CancellationTokenSource.Cancel();
                scriptData.CancellationTokenSource.Dispose();
            }

            runningScripts.Remove(reference);

            OnScriptRemoved(reference);
            return true;
        }

        public HashSet<IProgrammable> GetInstances() => runningScripts.Keys.ToHashSet();

        public bool Inject(RuntimeEnvironment runtimeEnvironment)
        {
            if (string.IsNullOrEmpty(runtimeEnvironment.Code))
                return false;

            if (runtimeEnvironment.Reference is null)
                throw new ArgumentNullException(nameof(runtimeEnvironment.Reference));

            // Remove previous script if any
            InternalRemove(runtimeEnvironment.Reference);

            // Create the script and load the code
            var script = new Script(coreModules);

            // Inject Public Dynamic members
            foreach (var (type, description) in publicDynamicMethods)
                UserData.RegisterType(type, description);

            // Inject Public Static members
            foreach (var (type, methodsDictionary) in publicStaticMembers)
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
            foreach (var luaStaticClass in luaStaticClasses.SelectMany(x => x.Value))
                script.DoString(luaStaticClass);

            // Inject Objects
            foreach (var luaObject in luaObjects)
                luaObject.Register(script);

            // Inject self
            publicDynamicMethods.TryGetValue(runtimeEnvironment.GetType(), out var standardUserDataDescriptor);
            standardUserDataDescriptor ??= new StandardUserDataDescriptor(runtimeEnvironment.Reference.GetType(), InteropAccessMode.HideMembers);
            foreach (var selfMethod in AuthorizedHelper.ExtractSelfMethods(runtimeEnvironment.Reference))
            {
                var methodDescriptorTuple = EnvironmentUtils.ConvertMethodInfoToMethodDescriptor(selfMethod);
                standardUserDataDescriptor.AddMember(methodDescriptorTuple.Item1, methodDescriptorTuple.Item2);
            }
            script.Globals[SelfKeyword] = UserData.Create(runtimeEnvironment.Reference, standardUserDataDescriptor);

#if DEBUG
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Self injected methods");
            foreach (var memberDescriptor in standardUserDataDescriptor.Members) 
                stringBuilder.Append(string.Format("{0}\t" + memberDescriptor.Key, Environment.NewLine));
            Debug.Log(stringBuilder.ToString());
            stringBuilder.Clear();
#endif
            
            // Format code
            var formattedCode = Utils.FormatCode(runtimeEnvironment.Code);

            // Create the script process
            var cancellationTokenSource = new CancellationTokenSource();
            var scriptData = new ScriptData
            {
                Script = script,
                CancellationTokenSource = cancellationTokenSource,
                Task = ScriptProcess(runtimeEnvironment.Reference, formattedCode, script, cancellationTokenSource.Token)
            };

            runningScripts.Add(runtimeEnvironment.Reference, scriptData);

            logger?.AddLog(runtimeEnvironment.Reference, LogType.Information, "Script injected");
            OnScriptAdded(runtimeEnvironment.Reference);
            return true;
        }

        public bool Remove(IProgrammable reference)
        {
            if (!InternalRemove(reference))
                return false;

            logger?.AddLog(reference, LogType.Information, "Script removed");
            return true;
        }
    }
}