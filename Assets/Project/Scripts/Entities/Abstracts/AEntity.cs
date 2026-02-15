using System;
using Project.Scripts.Interpreters;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Entities.Abstracts
{
    public abstract class AEntity : MonoBehaviour
    {
        public abstract float Radius
        {
            [AuthorizedHelper.AuthorizedMethod(true)]
            get;
        }
        
        public Vector2 Position
        {
            [AuthorizedHelper.AuthorizedMethod(true)]
            get => transform.position.XZToXYVector2();
        }


        public abstract EntityType EntityType
        {
            [AuthorizedHelper.AuthorizedMethod(true)]
            get;
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

        protected virtual void OnDisable()
        {
            EntitiesManager.Instance.Unsubscribe(this);
        }
    }
}