using System.Threading.Tasks;
using JetBrains.Annotations;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using UnityEngine;

namespace Project.Scripts.Services.Components
{
    public interface IMover
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void LookInDirection(Vector2 direction);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void LookAt(AEntity entity);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void MoveInDirection(Vector2 direction, float? speed = null);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void MoveToPosition(Vector2 position);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public Task WaitMoveToPosition(Vector2 position);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void Follow(AEntity entity);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void TurnAroundPoint(Vector2 center, bool clockWise = false, float? radius = null);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void TurnAround(AEntity entity, bool clockWise = true);
    }
}