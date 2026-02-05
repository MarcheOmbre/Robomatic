using System;
using System.Linq;

namespace Project.Scripts.Utils
{
    public static class EnumExtensions
    {
        public static int CountFlags<T>(this T flag) where T : Enum
        {
            var values = Enum.GetValues(flag.GetType()).Cast<T>();
            return values.Count(value => flag.HasFlag(value));
        }
    }
}