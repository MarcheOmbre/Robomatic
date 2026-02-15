using System.Linq;
using JetBrains.Annotations;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Player;

namespace Project.Scripts.Interpreters.Libraries
{
    public static class World
    {
        [UsedImplicitly]
        public static Robot Me
        {
            [AuthorizedHelper.AuthorizedMethod] get => EntitiesManager.Instance.Entities.OfType<Robot>().FirstOrDefault();
        }

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static AEntity GetEntity(int index = 0)
        {
            if(index < 0 || index >= EntitiesManager.Instance.Entities.Count)
                return null;

            return EntitiesManager.Instance.Entities[index];
        }

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static AEntity GetEntity(EntityType type, int index = 0)
        {
            var entities = EntitiesManager.Instance.Entities.Where(e => e.EntityType == type).ToList();
            
            if(index < 0 || index >= entities.Count)
                return null;

            return entities[index];
        }
    }
}