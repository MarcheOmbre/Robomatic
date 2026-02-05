using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using Project.Scripts.Services.Components;
using UnityEngine;

namespace Project.Scripts.Player
{
    public class Player : AEntity
    {
        public override EntityType EntityType => EntityType.Player;

        public IMover Mover
        {
            [AuthorizedHelper.AuthorizedMethod] 
            get => mover;
        }


        [SerializeField] private PlayerConfiguration configuration;
        [SerializeField] private CharacterController characterController;
        
        private CharacterControllerMover mover;

        private void Awake() => mover = new CharacterControllerMover(characterController, configuration);
    }
}