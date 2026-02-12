using UnityEngine;

namespace Project.Scripts.Utils
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                instance ??= FindFirstObjectByType<T>();
                return instance;
            }
        }
        
        private static T instance;

        protected virtual void Awake()
        {
            if (instance == null || instance == this)
                return;
            
            Debug.LogWarning("Two or more instances of singleton " + typeof(T) + " found. Destroying the next one.", gameObject);
            Destroy(this);
        }
    }
}