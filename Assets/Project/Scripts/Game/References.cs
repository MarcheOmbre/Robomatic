using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Game
{
    public class References : MonoBehaviourSingleton<References>
    {
        public GameManager GameManager => gameManager;
        
        [SerializeField] private GameManager gameManager;
    }
}