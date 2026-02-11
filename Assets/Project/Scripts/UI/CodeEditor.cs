using System;
using Project.Scripts.Interpreters;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Project.Scripts.UI
{
    public class CodeEditor : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
    
        private TextField codeTextElement;
        private Button runButton;
        private Button stopButton;
        
        private void Awake()
        {
            codeTextElement = document.rootVisualElement.Q<TextField>("code_editor");
            runButton = document.rootVisualElement.Q<Button>("button_run");
            stopButton = document.rootVisualElement.Q<Button>("button_stop");
            
            stopButton.SetEnabled(false);
        }

        private void OnEnable()
        {
            runButton.clicked += Run;
            stopButton.clicked += Stop;
        }
        
        private void OnDisable()
        {
            Stop();
            
            runButton.clicked -= Run;
            stopButton.clicked -= Stop;
        }

        private async void Run()
        {
            try
            {
                runButton.SetEnabled(false);
                stopButton.SetEnabled(true);
            
                await Interpreter.Instance.Run(codeTextElement.text);
            
                runButton.SetEnabled(true);
                stopButton.SetEnabled(false);
            }
            catch (Exception e)
            {
                if(e is not OperationCanceledException)
                    Debug.LogException(e);
            }
        }

        private void Stop()
        {
            Interpreter.Instance.Stop();
            
            runButton.SetEnabled(true);
            stopButton.SetEnabled(false);
        }
    }
}
