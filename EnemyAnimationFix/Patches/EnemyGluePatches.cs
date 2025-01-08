using Enemies;
using HarmonyLib;
using SNetwork;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyGluePatches
    {
        [HarmonyPatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.AddToTotalGlueVolume))]
        [HarmonyPostfix]
        private static void SyncGlueData(Dam_EnemyDamageBase __instance, GlueGunProjectile? proj, GlueVolumeDesc volume)
        {
            if (!SNet.IsMaster) return;

            pMiniDamageData data = default;
            float mod = proj != null ? proj.EffectMultiplier : 1f;
            data.damage.Set((volume.volume + volume.expandVolume) * mod, 100f);
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
