using JetBrains.Annotations;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using UnityEngine;

namespace Project.Scripts.Entities
{
    public class Player : AEntity
    {
        private const float FollowPositionOffset = 3f;
        private const float FollowRotationOffset = 10f;


        public override EntityType EntityType => EntityType.Player;


        [SerializeField] [Min(0)] private float translationSpeed = 5f;
        [SerializeField] [Min(0)] private float rotationSpeed = 100f;
        [SerializeField] private CharacterController controller;


        [UsedImplicitly]
        [AuthorizedMethod]
        public void Move() => controller.Move(transform.forward * translationSpeed * Time.deltaTime);

        [UsedImplicitly]
        [AuthorizedMethod]
        public void RotateLeft() => transform.rotation *= Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);

        [UsedImplicitly]
        [AuthorizedMethod]
        public void RotateRight() => transform.rotation *= Quaternion.Euler(0, -rotationSpeed * Time.deltaTime, 0);

        [UsedImplicitly]
        [AuthorizedMethod]
        public void RotateToward(AEntity entity)
        {
            if (entity == null || entity == this)
                return;

            var desiredForwardDirection = (entity.transform.position - transform.position).normalized;
            var signedAngle = Vector3.SignedAngle(transform.forward, desiredForwardDirection, Vector3.up);

            if (Mathf.Abs(signedAngle) <= FollowRotationOffset)
                return;

            var maxSpeed = rotationSpeed * Time.deltaTime;
            transform.rotation *= Quaternion.Euler(0, Mathf.Clamp(signedAngle, -maxSpeed, maxSpeed), 0);
        }

        [UsedImplicitly]
        [AuthorizedMethod]
        public void Follow(AEntity entity)
        {
            if (entity == null || entity == this)
                return;

            RotateToward(entity);

            var distance = Vector3.ProjectOnPlane(transform.position - entity.transform.position, Vector3.up).magnitude;
            if (distance > FollowPositionOffset)
                Move();
        }
    }
}