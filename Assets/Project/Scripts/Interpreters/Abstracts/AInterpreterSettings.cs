using Project.Scripts.Interpreters.Interfaces;
using UnityEngine;

namespace Project.Scripts.Interpreters.Abstracts
{
    public abstract class AInterpreterSettings : ScriptableObject
    {
        public IInterpreterService Service { get; private set; }

        
        private void Init() => Service = GetInterpreterService();

        protected virtual void Awake() => Init();
        
        protected virtual void OnValidate() => Init();
        
        
        protected abstract IInterpreterService GetInterpreterService();
    }
}