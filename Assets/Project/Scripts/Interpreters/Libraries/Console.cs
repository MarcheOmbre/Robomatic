using JetBrains.Annotations;

namespace Project.Scripts.Interpreters.Libraries
{
    public static class Console
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static void Debug(string value) => UnityEngine.Debug.Log(value);
    }
}