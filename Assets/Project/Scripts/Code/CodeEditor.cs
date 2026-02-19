using System;
using Project.Scripts.Interpreters;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Log;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Logger = Project.Scripts.Interpreters.Log.Logger;
using LogType = Project.Scripts.Interpreters.Log.LogType;

namespace Project.Scripts.Code
{
    public class CodeEditor
    {
        public IProgrammable CurrentProgrammable { get; private set; }


        private readonly UIDocument document;
        private readonly IInterpreterService interpreterService;
        private readonly Logger logger;

        private readonly TextField nameTextField;
        private readonly TextField codeTextField;
        private readonly Label consoleLabel;
        private readonly Button injectButton;
        private readonly Button offButton;

        private string programmableName;


        public CodeEditor(UIDocument document, IInterpreterService interpreterService, Logger logger)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.interpreterService = interpreterService ?? throw new ArgumentNullException(nameof(interpreterService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            nameTextField = document.rootVisualElement.Q<TextField>("text_field_name");
            codeTextField = document.rootVisualElement.Q<TextField>("text_field_code");
            consoleLabel = document.rootVisualElement.Q<Label>("label_console");
            injectButton = document.rootVisualElement.Q<Button>("button_inject");
            offButton = document.rootVisualElement.Q<Button>("button_off");

            interpreterService.OnScriptAdded += OnScriptAdded;
            interpreterService.OnScriptRemoved += OnScriptRemoved;
            logger.OnLogAdded += OnLogAdded;

            consoleLabel.enableRichText = true;
            
            Close();
        }

        ~CodeEditor()
        {
            logger.OnLogAdded -= OnLogAdded;
            
            Close();
        }

        private void Inject()
        {
            CurrentProgrammable.Name = !string.IsNullOrEmpty(nameTextField.value) ? nameTextField.value : programmableName;
            CurrentProgrammable.Code = codeTextField.value;
            interpreterService.Inject(new RuntimeEnvironment
            {
                Code = CurrentProgrammable.Code,
                Reference = CurrentProgrammable
            });
        }
        
        public void Open(IProgrammable programmable)
        {
            CurrentProgrammable = programmable ?? throw new ArgumentNullException(nameof(programmable));
            programmableName = CurrentProgrammable.Name;
       
            consoleLabel.text = "";
            document.rootVisualElement.style.display = DisplayStyle.Flex;
            
            injectButton.clickable.clicked += Inject;
            offButton.clickable.clicked += Stop;
            
            nameTextField.value = CurrentProgrammable.Name;
            offButton.SetEnabled(interpreterService.GetInstances().Contains(CurrentProgrammable));

            foreach (var logData in logger.GetLogsForReference(CurrentProgrammable))
                OnLogAdded(logData);
        }

        public void Close()
        {
            CurrentProgrammable = null;

            document.rootVisualElement.style.display = DisplayStyle.None;

            injectButton.clickable.clicked -= Inject;
            offButton.clickable.clicked -= Stop;
        }
        
        private void Stop() => interpreterService.Remove(CurrentProgrammable);
        
        
        private void OnScriptAdded(IProgrammable reference)
        {
            if (reference == CurrentProgrammable)
                offButton.SetEnabled(true);
        }
        
        private void OnScriptRemoved(IProgrammable reference)
        {
            if (reference == CurrentProgrammable)
                offButton.SetEnabled(false);
        }
        
        private void OnLogAdded(LogData logData)
        {
            if(logData.Reference != CurrentProgrammable)
                return;
            
            var lineText = logData.Line.HasValue ? $"Line {logData.Line.Value}" : "Unknown line";
            var message = $"{logData.LogType}: {logData.Message} ({lineText})";
            
            if(logData.LogType == LogType.Error)
                message = $"<color=red>{message}</color>";
            else if(logData.LogType == LogType.Warning)
                message = $"<color=yellow>{message}</color>";

            message += "\n";
            
            consoleLabel.text += message;
            Debug.Log(consoleLabel.text);
        }
    }
}