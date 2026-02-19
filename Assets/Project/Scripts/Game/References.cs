using Project.Scripts.Entities;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Game
{
    public class References : MonoBehaviourSingleton<References>
    {
        public EntitiesManager EntitiesManager { get; } = new();
        
        public Camera GameCamera => gameCamera;


        [SerializeField] private Camera gameCamera;
    }
}