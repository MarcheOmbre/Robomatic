using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using UnityEngine;

namespace Project.Scripts.Entities
{
    public class Player : AEntity
    {
        public override EntityType EntityType => EntityType.Player;


        [SerializeField] private float speed = 5f;
        [SerializeField] private CharacterController controller;
        
        
        [Authorized]
        public void Move() => controller.Move(transform.forward * speed * Time.deltaTime);

        [Authorized]
        public void Rotate(int degrees) => transform.Rotate(Vector3.up, degrees);
    }
}
