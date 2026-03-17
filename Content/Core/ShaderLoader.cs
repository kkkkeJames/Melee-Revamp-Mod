using MeleeRevamp.Content.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace MeleeRevamp.Content.Core
{
    class ShaderLoader : IOrderedLoadable
    {
        public float Priority { get => 0.9f; }

        public void Load()
        {
            if (Main.dedServ)
                return;

            MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            var file = (TmodFile)info.Invoke(MeleeRevamp.Instance, null);
            var shaders = file.Where(n => n.Name.StartsWith("Content/Effects/") && n.Name.EndsWith(".xnb"));

            foreach (FileEntry entry in shaders)
            {
                var name = entry.Name.Replace(".xnb", "").Replace("Content/Effects/", "");
                var path = entry.Name.Replace(".xnb", "");
                LoadShader(name, path);
            }
        }

        public void Unload()
        {

        }

        public static void LoadShader(string name, string path)
        {
            var screenRef = new Ref<Effect>(MeleeRevamp.Instance.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }
    }
}
