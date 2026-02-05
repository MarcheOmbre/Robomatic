using Project.Scripts.Services;
using UnityEngine;

namespace Project.Scripts.Interpreters.Abstracts
{
    public abstract class AInterpreterSettings : ScriptableObject
    {
        public IInterpreterService Service { get; private set; }

        protected abstract IInterpreterService GetInterpreterService();

        
        private void Init() => Service = GetInterpreterService();

        protected virtual void Awake() => Init();
        
        protected virtual void OnValidate() => Init();
    }
}