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
            [AuthorizedHelper.AuthorizedPublicMethod(true)]
            get;
        }
        
        public Vector2 Position
        {
            [AuthorizedHelper.AuthorizedPublicMethod(true)]
            get => transform.position.XZToXYVector2();
        }


        public abstract EntityType EntityType
        {
            [AuthorizedHelper.AuthorizedPublicMethod(true)]
            get;
        }

        
        private EntitiesManager entitiesManager;


        internal void Initialize(EntitiesManager manager)
        {
            entitiesManager = manager;
        }
        
        protected virtual void OnValidate()
        {
            if (EntityType.HasMultipleFlags())
                throw new ArgumentException("Entity only support One Flag set.");
        }

        
        public void Despawn()
        {
            if(entitiesManager is null)
                throw new InvalidOperationException("Entity not initialized by any EntitiesManager.");
            
            entitiesManager.Despawn(this);
        }
    }
}