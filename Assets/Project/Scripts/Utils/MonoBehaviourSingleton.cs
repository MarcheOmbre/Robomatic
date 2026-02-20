using System;
using UnityEngine;

namespace Project.Scripts.Utils
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }


        protected virtual void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(this);
                throw new InvalidOperationException("A MonoBehaviourSingleton of type " + typeof(T) +
                                                    " found in scene. The second one will be destroyed.");    
            }

            Instance = GetComponent<T>();
        }
    }
}