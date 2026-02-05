using JetBrains.Annotations;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;

namespace Project.Scripts.Services.Components
{
    public interface IMover
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void Move(float? speed);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void Rotate(float angle);
        
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void RotateLeft(float? speed);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void RotateRight(float? speed);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void RotateToward(AEntity entity);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod(true)]
        public void Follow(AEntity entity);
    }
}