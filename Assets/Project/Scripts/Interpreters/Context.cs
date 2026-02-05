using System.Linq;
using Project.Scripts.Entities;

namespace Project.Scripts.Interpreters
{
    public static class Context
    {
        public static Player Me
        {
            [Authorized]
            get => EntitiesManager.Instance.Entities.OfType<Player>().FirstOrDefault();
        }

        [Authorized]
        public static void Debug(string value) => UnityEngine.Debug.Log(value);
    }
}