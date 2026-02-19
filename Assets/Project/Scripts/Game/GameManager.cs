using MoonSharp.Interpreter;
using Project.Scripts.Code;
using Project.Scripts.Game.Inputs;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Lua;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Project.Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        private const int MaxLogCount = 1000;
        
        
        [SerializeField] private UIDocument developmentEnvironmentDocument;
        [SerializeField] private References references;
        [SerializeField] private InputActionReference programmableTriggerActionReference;
        [SerializeField] private InputActionReference programmableScreenPositionActionReference;

        private readonly Interpreters.Log.Logger logger = new(MaxLogCount);
        private IInterpreterService interpreter;
        private CodeEditor codeEditor;
        private ProgrammableInput programmableInput;


        private void Awake()
        {
            interpreter = new LuaInterpreterService(CoreModules.Preset_HardSandbox, logger);
            codeEditor = new CodeEditor(developmentEnvironmentDocument, interpreter, logger);
            
 
            programmableInput = new ProgrammableInput(programmableTriggerActionReference.action,
                programmableScreenPositionActionReference.action, references.GameCamera, codeEditor);
            
            programmableInput.Enable();
        }
    }
}