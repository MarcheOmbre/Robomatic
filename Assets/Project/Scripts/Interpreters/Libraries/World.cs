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
        [AuthorizedHelper.AuthorizedMethod]
        public static AEntity GetEntity(int index = 0)
        {
            if(index < 0 || index >= References.Instance.EntitiesManager.Entities.Count)
                return null;

            return References.Instance.EntitiesManager.Entities[index];
        }

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static AEntity GetEntity(EntityType type, int index = 0)
        {
            var entities = References.Instance.EntitiesManager.Entities.Where(e => e.EntityType == type).ToList();
            
            if(index < 0 || index >= entities.Count)
                return null;

            return entities[index];
        }
    }
}