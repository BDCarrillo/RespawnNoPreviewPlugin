using HarmonyLib;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using SpaceEngineers.Game.GUI;
using SpaceEngineers.Game.World;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Network;
using VRage.Utils;

namespace RespawnMenuNoPreview
{
    public static class Patches
    {
        [HarmonyPatch(typeof(MyGuiScreenMedicals), "ShowPreview")]
        public static class Patch_MyGuiScreenMedicals_ShowPreview
        {
            public static bool Prefix(MyGuiScreenMedicals __instance, bool ___m_isMultiplayerReady, object ___m_selectedRowData, bool ___m_selectedRowIsStreamable, long ___m_lastMedicalRoomId, int ___m_showPreviewTime)
            {
                if (___m_selectedRowData is MySpaceRespawnComponent.MyRespawnPointInfo myRespawnPointInfo)
                {
                    ___m_showPreviewTime = 0;
                    ___m_selectedRowIsStreamable = true;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
                    __instance.ShowBlackground();
                    ___m_lastMedicalRoomId = 0L;
                    ___m_isMultiplayerReady = false;
                    //MySession.RequestVicinityCache(myRespawnPointInfo.MedicalRoomGridId); //This was the bugger that takes forever to syncas it calls for voxel data
                    if (!Sync.IsServer && MyEntities.EntityExists(myRespawnPointInfo.MedicalRoomGridId))
                    {
                        typeof(MyGuiScreenMedicals).GetMethod("RequestConfirmation", AccessTools.all).Invoke(__instance, new object[] { });
                        return false;
                    }
                    //TODO look into altering replication range?  MyReplicationClient.SetClientReplicationRange(float)
                    typeof(MyGuiScreenMedicals).GetMethod("RequestReplicable", AccessTools.all).Invoke(__instance, new object[] { myRespawnPointInfo.MedicalRoomGridId });
                    MyEntities.OnEntityAdd += entity => doThing(__instance, entity);
                    return false;

                }
                return true;
            }

            private static void doThing(MyGuiScreenMedicals __instance, MyEntity entity) // TY Terif!
            {
                typeof(MyGuiScreenMedicals).GetMethod("OnEntityStreamedIn", AccessTools.all).Invoke(__instance, new object[] { entity });
            }
        }
    }
}
