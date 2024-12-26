using Enemies;
using HarmonyLib;
using SNetwork;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyGluePatches
    {
        [HarmonyPatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.ReceiveGlueDamage))]
        [HarmonyPostfix]
        private static void SyncGlueData(Dam_EnemyDamageBase __instance, pMiniDamageData data)
        {
            if (SNet.IsMaster)
                __instance.m_glueDamagePacket.Send(data, SNet_ChannelType.GameNonCritical);
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.ActivateState))]
        [HarmonyPrefix]
        private static bool StopClientStateSwitch()
        {
            return SNet.IsMaster;
        }
    }
}
