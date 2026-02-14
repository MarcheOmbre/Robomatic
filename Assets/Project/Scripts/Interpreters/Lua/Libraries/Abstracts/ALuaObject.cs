using MoonSharp.Interpreter;

namespace Project.Scripts.Interpreters.Lua.Libraries.Abstracts
{
    public abstract class ALuaObject
    {
        public abstract string Name { get; }

        public abstract void Register(Script script);
    }
}