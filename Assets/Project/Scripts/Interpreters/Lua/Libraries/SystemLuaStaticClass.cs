using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;

namespace Project.Scripts.Interpreters.Lua.Libraries
{
    public class SystemLuaStaticClass : ALuaStaticClass
    {
        public override string Name => "System";


        private readonly Method[] methods =
        {
            new(
                "wait",
                @"function(condition, ...)
                        
                        while(condition(...) == false) 
                        do
                            coroutine.yield()
                        end

                    end")
        };


        protected override Method[] GetMethods() => methods;
    }
}