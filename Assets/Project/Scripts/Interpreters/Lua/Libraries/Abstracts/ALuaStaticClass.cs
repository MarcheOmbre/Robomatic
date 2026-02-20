using System;
using System.Linq;

namespace Project.Scripts.Interpreters.Lua.Libraries.Abstracts
{
    public abstract class ALuaStaticClass
    {
        protected class Method
        {
            public string Name { get; }
            public string Code { get; }


            public Method(string name, string code)
            {
                Name = name;
                Code = code;
            }
        }
        
        public abstract string Name { get; }

        protected abstract Method[] GetMethods();
        

        public string[] ExtractMethods()
        {
            var methods = GetMethods();
            if(methods is not {Length: > 0})
                throw new ArgumentException("Methods cannot be null.");
            
            return methods.Where(x => x != null).Select(x =>
            {
                var generatedCode = Name + " = {}\n";
                generatedCode += x.Code.Replace("function", $"function {Name}.{x.Name}");
                return generatedCode;
            }).ToArray();
        }
    }
}