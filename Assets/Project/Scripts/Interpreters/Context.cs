using System.Linq;
using JetBrains.Annotations;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;

namespace Project.Scripts.Interpreters
{
    public static class Context
    {
        [UsedImplicitly]
        public static Player.Player Me
        {
            [AuthorizedHelper.AuthorizedMethod] get => EntitiesManager.Instance.Entities.OfType<Player.Player>().FirstOrDefault();
        }

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static AEntity GetFirstEntityOfType(EntityType type) =>
            EntitiesManager.Instance.Entities.FirstOrDefault(e => e.EntityType == type);

        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static void Debug(string value) => UnityEngine.Debug.Log(value);
    }
}