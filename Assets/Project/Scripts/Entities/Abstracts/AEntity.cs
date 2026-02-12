using System;
using JetBrains.Annotations;
using Project.Scripts.Interpreters;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Entities.Abstracts
{
    [RequireComponent(typeof(Collider))]
    public abstract class AEntity : MonoBehaviour
    {
        public Collider MainCollider { get; private set; }


        public abstract EntityType EntityType { get; }

        [UsedImplicitly]
        public int Type
        {
            [AuthorizedHelper.AuthorizedMethod(true)]
            get => (int)EntityType;
        }
        
        
        protected virtual void OnValidate()
        {
            if (EntityType.HasMultipleFlags())
                throw new ArgumentException("Entity only support One Flag set.");
        }

        protected virtual void OnEnable()
        {
            EntitiesManager.Instance.Subscribe(this);
        }

        protected virtual void Awake()
        {
            MainCollider = GetComponent<Collider>();
        }

        protected virtual void OnDisable()
        {
            EntitiesManager.Instance.Unsubscribe(this);
        }
    }
}