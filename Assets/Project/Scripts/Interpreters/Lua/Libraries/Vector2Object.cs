using System;
using MoonSharp.Interpreter;
using Project.Scripts.Interpreters.Lua.Libraries.Abstracts;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua.Libraries
{
    public class Vector2Object : AGenericLuaObject<Vector2>
    {
        protected override DynValue ConvertToDynValue(Script script, Vector2 value)
        {
            var x = DynValue.NewNumber(value.x);
            var y = DynValue.NewNumber(value.y);
            return DynValue.NewTable(script, x, y);
        }

        protected override Vector2 ConvertToClrObject(DynValue value)
        {
            var table = value.Table;
            var x = (float)(Double)table[1];
            var y = (float)(Double)table[2];
            return new Vector2(x, y);
        }
    }
}