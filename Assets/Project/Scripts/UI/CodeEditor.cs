using System;
using Project.Scripts.Interpreters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class CodeEditor : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button runButton;
        [SerializeField] private Button stopButton;

        private void Awake()
        {
            stopButton.interactable = false;
        }

        private void OnEnable()
        {
            runButton.onClick.AddListener(Run);
            stopButton.onClick.AddListener(Stop);
        }
        
        private void OnDisable()
        {
            Stop();
            
            runButton.onClick.RemoveListener(Run);
            stopButton.onClick.RemoveListener(Stop);
        }

        private async void Run()
        {
            try
            {
                runButton.interactable = false;
                stopButton.interactable = true;
            
                await Interpreter.Instance.Run(inputField.text);
            
                runButton.interactable = true;
                stopButton.interactable = false;
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
            runButton.interactable = true;
            stopButton.interactable = false;
        }
    }
}
