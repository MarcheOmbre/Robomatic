using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;

namespace Project.Scripts.Interpreters.Lua.Libraries
{
    public class ConsoleStaticClass : ALuaStaticClass
    {
        public override string Name => "Console";


        private readonly Method[] methods =
        {
            new(
                "write",
                @"function(message, type)
                        
                        Internal.log(self, message, type)

                    end")
        };


        protected override Method[] GetMethods() => methods;
    }
}