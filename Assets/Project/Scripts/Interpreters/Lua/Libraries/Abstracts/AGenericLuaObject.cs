using MoonSharp.Interpreter;

namespace Project.Scripts.Interpreters.Lua.Libraries.Abstracts
{
    public abstract class AGenericLuaObject<T> : ALuaObject
    {
        public sealed override string Name => typeof(T).Name;
        
        

        protected abstract DynValue ConvertToDynValue(Script script, T value);

        protected abstract T ConvertToClrObject(DynValue value);
        
        
        public override void Register(Script script)
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(T),
                dynVal => ConvertToClrObject(dynVal));

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<T>(ConvertToDynValue);
        }
    }
}