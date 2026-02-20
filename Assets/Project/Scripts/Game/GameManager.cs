using MoonSharp.Interpreter;
using Project.Scripts.Code;
using Project.Scripts.Entities;
using Project.Scripts.Game.Inputs;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Lua;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Logger = Project.Scripts.Interpreters.Log.Logger;

namespace Project.Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        private const int MaxLogCount = 1000;
        
        
        public EntitiesManager EntitiesManager { get; } = new();
        
        public Camera GameCamera => gameCamera;
        
        public CodeEditor CodeEditor { get; private set; }
        
        public Logger Logger { get; } = new(MaxLogCount);


        [SerializeField] private UIDocument developmentEnvironmentDocument;
        [SerializeField] private InputActionReference programmableTriggerActionReference;
        [SerializeField] private InputActionReference programmableScreenPositionActionReference;
        [SerializeField] private Camera gameCamera;
        
        private IInterpreterService interpreter;
        private ProgrammableInput programmableInput;


        private void Awake()
        {
            // Initialize
            interpreter = new LuaInterpreterService(CoreModules.Preset_HardSandbox | CoreModules.Coroutine, Logger);
            CodeEditor = new CodeEditor(developmentEnvironmentDocument, interpreter, Logger);
            programmableInput = new ProgrammableInput(programmableTriggerActionReference.action,
                programmableScreenPositionActionReference.action, gameCamera, CodeEditor);
            
            // Setup
            programmableInput.Enable();
            EntitiesManager.ScanSceneEntities();
        }
    }
}