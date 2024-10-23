using EnemyAnimationFix.Networking.Notify;
using HarmonyLib;
using SNetwork;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class NetworkingPatches
    {
        [HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnFoundMaster))]
        [HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnFoundNewMasterDuringMigration))]
        [HarmonyPostfix]
        private static void Post_Joined()
        {
            if (!SNet.IsMaster)
                NotifyManager.SendNotify(SNet.Master);
            else
                NotifyManager.SetMaster();
        }

        [HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnPlayerJoinedSessionHub))]
        [HarmonyPostfix]
        private static void Post_Joined(SNet_Player player)
        {
            if (SNet.IsMaster && !player.IsLocal)
                NotifyManager.SendNotify(player);
        }

        [HarmonyPatch(typeof(SNet_SessionHub), nameof(SNet_SessionHub.OnLeftLobby))]
        [HarmonyPatch(typeof(SNet_SessionHub), nameof(SNet_SessionHub.RemovePlayerFromSession))]
        [HarmonyPrefix]
        private static void Pre_Eject(SNet_Player player)
        {
            if (SNet.IsMaster)
                NotifyManager.RemoveClient(player);
        }

        [HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnResetSession))]
        [HarmonyPrefix]
        private static void Pre_OnReset()
        {
            NotifyManager.Reset();
        }
    }
}
