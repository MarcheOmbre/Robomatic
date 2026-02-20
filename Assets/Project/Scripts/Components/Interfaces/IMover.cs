using JetBrains.Annotations;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using UnityEngine;

namespace Project.Scripts.Components.Interfaces
{
    public interface IMover
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool LookAt(Vector2 targetDirection);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool LookAt(float x, float y) => LookAt(new Vector2(x, y));
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool LookAt(AEntity targetEntity);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public void MoveToward(Vector2 targetDirection);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public void MoveToward(int x, int y) => MoveToward(new Vector2(x, y));
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool Reach(Vector2 targetPosition);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool Reach(int x, int y) => Reach(new Vector2(x, y));

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public bool Reach(AEntity targetEntity);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public void TurnAround(Vector2 center, bool clockWise = false, float? radius = null);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public void TurnAround(int x, int y, bool clockWise = false, float? radius = null) => TurnAround(new Vector2(x, y), clockWise, radius);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod(true)]
        public void TurnAround(AEntity targetEntity, bool clockWise = true, float? radius = null);
    }
}