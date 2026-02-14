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
            get => transform.position.ToVector2();
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

        [AuthorizedHelper.AuthorizedMethod(true)]
        public bool IsInRange(AEntity entity)
        {
            if(entity is null)
                return false;
            
            return Vector2.Distance(transform.position, entity.transform.position) <= Radius + entity.Radius;
        }
    }
}