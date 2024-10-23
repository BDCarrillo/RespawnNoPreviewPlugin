using HarmonyLib;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using SpaceEngineers.Game.World;
using System.Reflection;
using System.Text;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Network;
using VRageMath;
using static SpaceEngineers.Game.World.MySpaceRespawnComponent;

namespace RespawnMenuNoPreview
{
    public static class Patches
    {

        [HarmonyPatch(typeof(MyGuiScreenMedicals), "ShowPreview")]
        public static class Patch_MyGuiScreenMedicals_ShowPreview
        {
            public static long m_requestedReplicable;
            public static MyGuiScreenMedicals instance;
            public static bool Prefix(MyGuiScreenMedicals __instance, ref bool ___m_isMultiplayerReady, object ___m_selectedRowData, ref bool ___m_selectedRowIsStreamable, ref long ___m_lastMedicalRoomId, ref int ___m_showPreviewTime)
            {
                ___m_showPreviewTime = 0;
                if (___m_selectedRowData is MyRespawnPointInfo myRespawnPointInfo)
                {
                    ___m_selectedRowIsStreamable = true;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
                    ___m_lastMedicalRoomId = 0L;
                    ___m_isMultiplayerReady = false;
                    if (!Sync.IsServer && MyEntities.EntityExists(myRespawnPointInfo.MedicalRoomGridId))
                    {
                        typeof(MyGuiScreenMedicals).GetMethod("RequestConfirmation", AccessTools.all).Invoke(__instance, new object[] { });
                        return false;
                    }
                    typeof(MyGuiScreenMedicals).GetMethod("RequestReplicable", AccessTools.all).Invoke(__instance, new object[] { myRespawnPointInfo.MedicalRoomGridId, myRespawnPointInfo.MedicalRoomId });
                    instance = __instance;
                    m_requestedReplicable = myRespawnPointInfo.MedicalRoomGridId;
                    MyEntities.OnEntityAdd += OnEntityStreamedIn;
                    return false;
                }
                return true;
            }
            private static void OnEntityStreamedIn(MyEntity entity)
            {
                if (entity.EntityId == m_requestedReplicable)
                {
                    typeof(MyGuiScreenMedicals).GetMethod("RequestConfirmation", AccessTools.all).Invoke(instance, new object[] { });
                    MyEntities.OnEntityAdd -= OnEntityStreamedIn;
                }
            }
        }

        [HarmonyPatch(typeof(MyGuiScreenMedicals), "RefreshMedicalRooms")] //Color items in sync green
        public static class Patch_MyGuiScreenMedicals_Index
        {
            public static void Postfix(MyGuiScreenMedicals __instance, ref MyGuiControlTable ___m_respawnsTable)
            {
                if (___m_respawnsTable == null || ___m_respawnsTable.RowsCount == 0)
                    return;

                for (int i = 0; i < ___m_respawnsTable.RowsCount; i++)
                {
                    var rowtest = ___m_respawnsTable.GetRow(i);
                    if (rowtest.UserData is MyRespawnPointInfo myRespawnPointInfo)
                    {
                        var cell = rowtest.GetCell(1);
                        if (MyEntities.EntityExists(myRespawnPointInfo.MedicalRoomGridId))
                            cell.TextColor = Color.Green;
                        else
                            cell.TextColor = Color.Red;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MyGuiScreenMedicals), "OnTableItemSelected")] //Prevent the fake blank spawn from allowing a suit spawn
        public static class Patch_MyGuiScreenMedicals_Update
        {
            public static void Postfix(MyGuiControlTable ___m_respawnsTable, MyGuiControlButton ___m_respawnButton, object ___m_selectedRowData)
            {
                if (___m_respawnsTable == null || ___m_respawnsTable.RowsCount == 0 || ___m_respawnsTable.SelectedRow == null)
                    return;
                if (___m_respawnsTable.SelectedRow.GetCell(0).Text.EqualsStrFast("  ---  "))
                {
                    ___m_respawnButton.Enabled = false;
                    ___m_selectedRowData = null;
                }
            }
        }

        [HarmonyPatch(typeof(MyGuiScreenMedicals), "ShowEmptyPreview")] //Keep SE from flushing replication on the dummy spawn

        public static class Patch_MyGuiScreenMedicals_SaveSomeNetwork
        {
            public static bool Prefix(MyGuiScreenMedicals __instance, MyGuiControlTable ___m_respawnsTable, ref bool ___m_selectedRowIsStreamable)
            {
                if (___m_respawnsTable == null || ___m_respawnsTable.RowsCount == 0 || ___m_respawnsTable.SelectedRow == null)
                    return true;
                if (___m_respawnsTable.SelectedRow.GetCell(0).Text.EqualsStrFast("  ---  "))
                {
                    typeof(MyGuiScreenMedicals).GetMethod("ShowBlackground", AccessTools.all).Invoke(__instance, new object[] { });
                    ___m_selectedRowIsStreamable = false;
                    return false;
                }
                return true;
            }
        }

        
        [HarmonyPatch(typeof(MyGuiScreenMedicals), "RequestReplicable")] //Tinkering with layer number and suppressing unrequest

        public static class Patch_MyGuiScreenMedicals_RequestReplicable
        {
            public static bool Prefix(MyGuiScreenMedicals __instance, long replicableId, long ___m_requestedReplicable, long medicalRoomId)
            {
                if (___m_requestedReplicable != replicableId)
                {
                    ___m_requestedReplicable = replicableId;
                    if (MyMultiplayer.ReplicationLayer is MyReplicationClient myReplicationClient)
                    {
                        myReplicationClient.RequestReplicable(___m_requestedReplicable, 0, true, 0, medicalRoomId);
                    }
                }
                return false;
            }
        }
        

        [HarmonyPatch]
        public static class Patch_MyGuiScreenMedicals_AddBlank //Add a fake, blank spawn location
        {
            public static MethodBase TargetMethod()
            {
                return AccessTools.FirstMethod(typeof(MyGuiScreenMedicals),
                    m => m.Name.Contains("<RefreshMedicalRooms>g__AddMedicalRespawnPoints"));
            }

            public static void Postfix(MyGuiControlTable ___m_respawnsTable)
            {
                MyGuiControlTable.Row row = new MyGuiControlTable.Row("Textures\\GUI\\Icons\\RespawnShips\\RespawnSuit.png");
                row.AddCell(new MyGuiControlTable.Cell("  ---  "));
                row.AddCell(new MyGuiControlTable.Cell("  ----  "));
                ___m_respawnsTable.Insert(0, row);
            }
        }
    }
}
