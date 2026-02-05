using System;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Services.Components;
using UnityEngine;

namespace Project.Scripts.Player
{
    public class CharacterControllerMover: IMover
    {
        private readonly CharacterController characterController;
        private readonly ISpeedConfiguration speedConfiguration;

        public CharacterControllerMover(CharacterController controller, ISpeedConfiguration configuration)
        {
            characterController = controller ?? throw new ArgumentNullException(nameof(controller), "Character controller cannot be null");
            speedConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Speed configuration cannot be null");
        }
        
        public void Move(float? speed)
        {
            if(characterController is null)
                throw new ArgumentNullException(nameof(characterController), "Character controller cannot be null");
            
            speed ??= speedConfiguration.TranslationSpeed;
            speed = Mathf.Clamp(speed.Value, 0, speedConfiguration.TranslationSpeed);
            
            var forwardPosition = Vector3.ProjectOnPlane(characterController.transform.forward, Vector3.up) * speed.Value;
            characterController.Move(Vector3.MoveTowards(Vector3.zero, forwardPosition, Time.deltaTime));
        }

        public void Rotate(float angle)
        {
            characterController.transform.rotation *= Quaternion.RotateTowards(Quaternion.identity, 
                new Quaternion(0, angle, 0, 0), speedConfiguration.RotationSpeed * Time.deltaTime);
        }

        public void RotateLeft(float? speed) => Rotate(-Mathf.Abs(speed ?? speedConfiguration.RotationSpeed));
        
        public void RotateRight(float? speed) => Rotate(Mathf.Abs(speed ?? speedConfiguration.RotationSpeed));
        
        public void RotateToward(AEntity entity)
        {
            if (entity == null || entity.gameObject == characterController.gameObject)
                return;

            var desiredForwardDirection = (entity.transform.position - characterController.transform.position).normalized;
            var angle = Vector3.SignedAngle(characterController.transform.forward, desiredForwardDirection, Vector3.up);
            
            if(angle == 0)
                return;
            
            Debug.Log(angle);
            if(angle < 0)
                RotateLeft(angle);
            else
                RotateRight(Mathf.Abs(angle));
        }
        
        public void Follow(AEntity entity)
        {
            if (entity == null || characterController.gameObject == entity.gameObject)
                return;

            RotateToward(entity);
            Move(Vector3.ProjectOnPlane(entity.transform.position - characterController.transform.position, Vector3.up).magnitude);
        }
    }
}