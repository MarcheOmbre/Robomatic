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
        private readonly ScrollView consoleScrollView;
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
            consoleScrollView = document.rootVisualElement.Q<ScrollView>("scroll_view_console");
            consoleLabel = document.rootVisualElement.Q<Label>("label_console");
            injectButton = document.rootVisualElement.Q<Button>("button_inject");
            offButton = document.rootVisualElement.Q<Button>("button_off");
            
            interpreterService.OnScriptAdded += OnScriptAdded;
            interpreterService.OnScriptRemoved += OnScriptRemoved;
            logger.OnLogAdded += OnLogAdded;
            
            this.document.rootVisualElement.style.display = DisplayStyle.None;
        }

        ~CodeEditor()
        {
            logger.OnLogAdded -= OnLogAdded;
            
            if(CurrentProgrammable != null)
                Close();
        }
        
        public void Open(IProgrammable programmable)
        {
            if (CurrentProgrammable != null)
                throw new ApplicationException("CodeEditor is already open");
            
            // Initialize the variables
            CurrentProgrammable = programmable ?? throw new ArgumentNullException(nameof(programmable));
            programmableName = CurrentProgrammable.Name;
            
            // Initialize fields
            nameTextField.value = CurrentProgrammable.Name;
            codeTextField.value = programmable.Code;
            consoleLabel.text = "";
            foreach (var logData in logger.GetLogsForReference(CurrentProgrammable))
                OnLogAdded(logData);
            offButton.SetEnabled(interpreterService.GetInstances().Contains(CurrentProgrammable));

            // Events
            injectButton.clickable.clicked += Inject;
            offButton.clickable.clicked += Stop;
            
            // Display the window
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void Close()
        {
            if (CurrentProgrammable == null)
                throw new ApplicationException("CodeEditor is not open");
            
            // Uninitialize the variables
            CurrentProgrammable = null;
            
            // Hide the window
            document.rootVisualElement.style.display = DisplayStyle.None;
            
            // Events
            injectButton.clickable.clicked -= Inject;
            offButton.clickable.clicked -= Stop;
        }

        
        private void Inject()
        {
            // Check name
            if (string.IsNullOrEmpty(nameTextField.value))
                nameTextField.value = programmableName;
            
            CurrentProgrammable.Name = nameTextField.value;
            CurrentProgrammable.Code = codeTextField.value;
            interpreterService.Inject(new RuntimeEnvironment
            {
                Code = CurrentProgrammable.Code,
                Reference = CurrentProgrammable
            });
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
        
        private async void OnLogAdded(LogData logData)
        {
            try
            {
                if(logData.Reference != CurrentProgrammable)
                    return;

                var message = logData.LogType.ToString();
                if (logData.Line.HasValue)
                    message += $" (l.{logData.Line.Value})";
                message += " : " + logData.Message;
            
                if(logData.LogType == LogType.Error)
                    message = $"<color=red>{message}</color>";
                else if(logData.LogType == LogType.Warning)
                    message = $"<color=yellow>{message}</color>";

                message += "\n";
            
                consoleLabel.text += message;
            
                // Necessary to wait until the ui has redrawn
                await Awaitable.EndOfFrameAsync();
                consoleScrollView.scrollOffset = Vector2.one * int.MaxValue;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}