using System;
using Project.Scripts.Code;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Game.Inputs
{
    public class ProgrammableInput
    {
        private readonly InputAction triggerAction;
        private readonly InputAction screenPositionAction;
        private readonly Camera camera;
        private readonly CodeEditor codeEditor;
        
        
        public ProgrammableInput(InputAction triggerAction, InputAction screenPositionAction, Camera camera, CodeEditor codeEditor)
        {
            this.triggerAction = triggerAction ?? throw new ArgumentNullException(nameof(triggerAction));
            this.screenPositionAction = screenPositionAction ?? throw new ArgumentNullException(nameof(screenPositionAction));
            this.camera = camera ?? throw new ArgumentNullException(nameof(camera));
            this.codeEditor = codeEditor ?? throw new ArgumentNullException(nameof(codeEditor));
        }

        public void Enable()
        {
            triggerAction.Enable();
            screenPositionAction.Enable();

            triggerAction.performed += OnTriggerActionPerformed;
        }
        
        public void Disable()
        {
            codeEditor.Close();
            
            triggerAction.Disable();
            screenPositionAction.Disable();
            
            triggerAction.performed -= OnTriggerActionPerformed;
        }

        private void OnTriggerActionPerformed(InputAction.CallbackContext _)
        {
            var position = screenPositionAction.ReadValue<Vector2>();
            
            // Check if the position is over UI
            if(UIExtensions.GetUIUnderPosition(position).Length > 0)
                return;
            
            // Ray on camera
            var ray = camera.ScreenPointToRay(position);
            Physics.Raycast(ray, out var hit);
            
            var isEditorOpen = codeEditor.CurrentProgrammable != null;
            
            // Close the editor and return if not IProgrammable selected
            if (!hit.transform || !hit.transform.TryGetComponent<IProgrammable>(out var programmable))
            {
                if(isEditorOpen)
                    codeEditor.Close();
                
                return;
            }

            // If the editor current IProgrammable is the same as the selected one, we skip
            if (programmable == codeEditor.CurrentProgrammable)
                return;
            
            if(isEditorOpen)
                codeEditor.Close();
            
            codeEditor.Open(programmable);
        }
    }
}