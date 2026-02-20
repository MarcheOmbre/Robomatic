using System.Linq;
using JetBrains.Annotations;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Game;

namespace Project.Scripts.Interpreters.Libraries
{
    public static class World
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod]
        public static AEntity GetEntity(int index = 0)
        {
            var entities = References.Instance.GameManager.EntitiesManager.Entities.ToArray();
            if(index < 0 || index >= entities.Length)
                return null;

            return entities[index];
        }

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedPublicMethod]
        public static AEntity GetEntity(EntityType type, int index = 0)
        {
            var entities = References.Instance.GameManager.EntitiesManager.Entities.Where(e => e.EntityType == type).ToArray();
            if(index < 0 || index >= entities.Length)
                return null;

            return entities[index];
        }
    }
}