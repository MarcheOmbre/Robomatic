using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Project.Scripts.Interpreters.Lua.Editor
{
    [ScriptedImporter(1, "lua")]
    public class LuaAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
            Debug.Log("LuaAssetImporter");
        }
    }
}