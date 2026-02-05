using System;

namespace Project.Scripts.Utils
{
    public static class EnumExtensions
    {
        public static bool HasMultipleFlags<T>(this T flag) where T : Enum
        {
            return (Convert.ToInt32(flag) & (Convert.ToInt32(flag) - 1)) != 0;
        }
    }
}