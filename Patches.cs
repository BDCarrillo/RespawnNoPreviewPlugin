using HarmonyLib;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System.Reflection;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace RespawnMenuNoPreview
{
    public static class Patches
    {
        
        [HarmonyPatch(typeof(MyGuiScreenMedicals), "ShowPreview")]
        public static class Patch_MyGuiScreenMedicals_ShowPreview
        {
            public static bool Prefix(MyGuiScreenMedicals __instance)
            {
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D(1000000.0));
                __instance.ShowBlackground();
                return false;                
            }
        }       
    }
}
