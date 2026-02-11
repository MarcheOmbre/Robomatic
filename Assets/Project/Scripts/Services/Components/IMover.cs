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
        public bool LookAt(Vector2 direction);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public bool LookAt(AEntity entity);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void MoveToward(Vector2 direction, float? speed = null);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public bool Reach(Vector2 position);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public bool Reach(AEntity entity);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void TurnAround(Vector2 center, bool clockWise = false, float? radius = null);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void TurnAround(AEntity entity, bool clockWise = true);
    }
}