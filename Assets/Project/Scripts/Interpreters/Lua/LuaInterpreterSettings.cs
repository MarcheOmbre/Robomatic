using System;
using MoonSharp.Interpreter;
using Project.Scripts.Interpreters.Abstracts;
using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua
{
    [Serializable]
    [CreateAssetMenu(fileName = "LuaInterpreterSettings", menuName = "Interpreter/Lua Interpreter Settings", order = 0)]
    public class LuaInterpreterSettings : AInterpreterSettings
    {
        [SerializeField] private CoreModules coreModules;
        [SerializeField] private TextAsset[] externalModules;
        [SerializeField] private bool debuggerEnabled;


        protected override IInterpreterService GetInterpreterService()
        {
            return new LuaInterpreterService(coreModules, externalModules, debuggerEnabled);
        }
    }
}