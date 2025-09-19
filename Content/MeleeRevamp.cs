using MeleeRevamp.Content.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MeleeRevamp.Content
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class MeleeRevamp : Mod
    {
        public static MeleeRevamp Instance { get; set; }
        private List<IOrderedLoadable> loadCache;
        public MeleeRevamp()
        {
            Instance = this;
        }
        public override void Load()
        {
            loadCache = new List<IOrderedLoadable>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable))) //load all interfaces
                {
                    var instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as IOrderedLoadable); //add that instance to load list
                }

                loadCache.Sort((n, t) => n.Priority.CompareTo(t.Priority)); //sort all interfaces by priority
            }

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load(); //load
                SetLoadingText("Loading " + loadCache[k].GetType().Name); //display all loaded interfaces
            }
        }

        public override void Unload()
        {
            if (loadCache is not null)
            {
                foreach (var loadable in loadCache)
                {
                    loadable.Unload();
                }

                loadCache = null;
            }
        }

        public static void SetLoadingText(string text)
        {
            var Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface")!.GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static)!;
            var UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress")!.GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance)!.GetSetMethod()!;

            UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
        }
    }
}
