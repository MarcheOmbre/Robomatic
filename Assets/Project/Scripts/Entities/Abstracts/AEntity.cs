using System;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Entities.Abstracts
{
    public abstract class AEntity : MonoBehaviour
    {
        public abstract EntityType EntityType { get; }
        
        
        private void OnValidate()
        {
            if (EntityType.CountFlags() != 1)
                throw new ArgumentException("Entity only support One Flag set.");
        }

        protected virtual void OnEnable()
        {
            EntitiesManager.Instance.Subscribe(this);
        }
        
        protected void OnDisable()
        {
            EntitiesManager.Instance.Unsubscribe(this);
        }
    }
}