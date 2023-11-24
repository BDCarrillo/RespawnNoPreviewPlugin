using System.Reflection;
using VRage.Plugins;
using HarmonyLib;

namespace RespawnMenuNoPreview
{
    public class Main : IPlugin
    {
        public void Init(object gameInstance)
        {
            //new Harmony("RespawnMenuNoPreview").PatchAll(Assembly.GetExecutingAssembly());
            new Harmony(typeof(Main).Namespace).PatchAll(typeof(Main).Assembly);
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }
    }
}
