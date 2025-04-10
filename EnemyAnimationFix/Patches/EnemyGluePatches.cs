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
            if (__instance.Owner.Locomotion.CurrentStateEnum == ES_StateEnum.StuckInGlue)
            {
                var stuck = __instance.Owner.Locomotion.StuckInGlue;
                if (stuck.m_glueFadeOutTriggered)
                {
                    float time = stuck.m_fadeOutDuration - (1f - __instance.AttachedGlueRel) * __instance.Owner.EnemyBalancingData.GlueFadeOutTime;
                    var appearance = __instance.Owner.Appearance;
                    appearance.m_lastGlueEnd = time / stuck.m_fadeOutDuration;
                    appearance.SetGlueAmount(0f, time);
                }
            }

            if (!SNet.IsMaster) return;

            pMiniDamageData data = default;
            float mod = proj != null ? proj.EffectMultiplier : 1f;
            data.damage.Set((volume.volume + volume.expandVolume) * mod, __instance.HealthMax);
            __instance.m_glueDamagePacket.Send(data, SNet_ChannelType.GameNonCritical);
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.ActivateState))]
        [HarmonyPrefix]
        private static bool StopClientStateSwitch()
        {
            return SNet.IsMaster;
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.SetAI))]
        [HarmonyPostfix]
        private static void FixFadeOutDuration(ES_StuckInGlue __instance)
        {
            // 1.5 is just a magic number that makes it line up better
            __instance.m_fadeOutDuration = (__instance.m_fadeOutDuration - __instance.m_fadeInDuration) * 1.5f;
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.ActivateState))]
        [HarmonyPostfix]
        private static void FixFadeOutTime(ES_StuckInGlue __instance)
        {
            __instance.m_fadeOutTimer = __instance.m_fadeInTimer;
        }
    }
}
