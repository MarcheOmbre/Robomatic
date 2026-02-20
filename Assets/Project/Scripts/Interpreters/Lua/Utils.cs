using System;
using MoonSharp.Interpreter;

namespace Project.Scripts.Interpreters.Lua
{
    public static class Utils
    {
        /// <summary>
        /// Inject yield at each loop and wrap the code in a function to avoid freezing.
        /// </summary>
        /// <param name="code">The Lua code to execute</param>
        public static string FormatCode(string code)
        {
            if (code is null)
                return string.Empty;

            // Inject yield at each loop
            code = code.Replace("do", "do coroutine.yield()");

            // Wrap the code in a function
            code = $"return function() {code} end";

            return code;
        }

        /// <summary>
        /// Returns the line of the error in the interpreter exception.
        /// </summary>
        /// <param name="exception">The exception where to extract the line</param>
        /// <returns></returns>
        public static int? ExtractLine(Exception exception)
        {
            if (exception is not InterpreterException interpreterException)
                return null;

            var decoratedMessage = interpreterException.DecoratedMessage;

            var firstIndex = decoratedMessage.IndexOf('(');
            var lastIndex = decoratedMessage.LastIndexOf(',');

            if (firstIndex >= 0 && lastIndex >= 0 &&
                int.TryParse(decoratedMessage.Substring(firstIndex + 1, lastIndex - firstIndex - 1), out var line))
                return line;

            return null;
        }
    }
}