using Project.Scripts.Components.Interfaces;
using UnityEngine;

namespace Project.Scripts.Game.Entities
{
    [CreateAssetMenu(fileName = "PlayerConfiguration", menuName = "Robot/Configuration")]
    public class PlayerConfiguration : ScriptableObject, ISpeedConfiguration
    {
        [SerializeField] [Min(0)] private float translationSpeed = 10f;
        [SerializeField] [Min(0)] private float rotationSpeed = 10f;

        public float TranslationSpeed => translationSpeed;
        public float RotationSpeed => rotationSpeed;
    }
}