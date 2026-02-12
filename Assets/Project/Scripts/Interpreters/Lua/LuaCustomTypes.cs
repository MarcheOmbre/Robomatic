using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua
{
    public abstract class CustomType<T>
    {
        protected abstract DynValue[] ConvertToDynValue(T value);

        protected abstract T ConvertToClrObject(DynValue value);


        public void Register()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(T),
                dynVal => ConvertToClrObject(dynVal));

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<T>((script, value) =>
                DynValue.NewTable(script, ConvertToDynValue(value)));
        }
    }

    public class CustomVector2 : CustomType<Vector2>
    {
        protected override DynValue[] ConvertToDynValue(Vector2 value)
        {
            var x = DynValue.NewNumber(value.x);
            var y = DynValue.NewNumber(value.y);
            return new[] { x, y };
        }

        protected override Vector2 ConvertToClrObject(DynValue value)
        {
            var table = value.Table;
            var x = (float)(Double)table[1];
            var y = (float)(Double)table[2];
            return new Vector2(x, y);
        }
    }

    public class CustomVector3 : CustomType<Vector3>
    {
        protected override DynValue[] ConvertToDynValue(Vector3 value)
        {
            var x = DynValue.NewNumber(value.x);
            var y = DynValue.NewNumber(value.y);
            var z = DynValue.NewNumber(value.z);
            return new[] { x, y, z };
        }

        protected override Vector3 ConvertToClrObject(DynValue value)
        {
            var table = value.Table;
            var x = (float)(Double)table[1];
            var y = (float)(Double)table[2];
            var z = (float)(Double)table[3];
            return new Vector3(x, y, z);
        }
    }
}