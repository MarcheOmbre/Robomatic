using System.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts.GameModes.Abstracts
{
    public abstract class AGameMode : ScriptableObject
    {
        public abstract void Initialize();
        
        public abstract Task Update();
        
        public abstract void Cleanup();
    }
}