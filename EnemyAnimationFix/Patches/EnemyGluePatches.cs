using Enemies;
using EnemyAnimationFix.Networking.Foam;
using HarmonyLib;
using SNetwork;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyGluePatches
    {
        [HarmonyPatch(typeof(Dam_EnemyDamageBase), nameof(Dam_EnemyDamageBase.AddToTotalGlueVolume))]
        [HarmonyPostfix]
        private static void SyncGlueData(Dam_EnemyDamageBase __instance)
        {
            if (__instance.Owner.Locomotion.CurrentStateEnum == ES_StateEnum.StuckInGlue)
            {
                var stuck = __instance.Owner.Locomotion.StuckInGlue;
                if (stuck.m_glueFadeOutTriggered)
                {
                    float time = (stuck.m_fadeOutDuration - stuck.m_fadeInDuration) * __instance.AttachedGlueRel * 1.44f;
                    var appearance = __instance.Owner.Appearance;
                    appearance.m_lastGlueEnd = time / stuck.m_fadeOutDuration;
                    appearance.SetGlueAmount(0f, time);
                }
            }

            if (!SNet.IsMaster) return;

            FoamManager.SendFoam(__instance.Owner, __instance.AttachedGlueRel);
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.ActivateState))]
        [HarmonyPrefix]
        private static bool StopClientStateSwitch()
        {
            return SNet.IsMaster;
        }

        [HarmonyPatch(typeof(ES_StuckInGlue), nameof(ES_StuckInGlue.DoStartStuckInGlue))]
        [HarmonyPostfix]
        private static void FixFadeOutTime(ES_StuckInGlue __instance)
        {
            // JFS - Should always be the same on host, but not touching it there just in case.
            if (!SNet.IsMaster)
                __instance.m_enemyAgent.Damage.m_attachedGlueVolume = __instance.m_enemyAgent.EnemyBalancingData.GlueTolerance;

            // Make visual fade out after the glue fade begins so it lines up better
            __instance.m_fadeOutTimer = __instance.m_fadeInTimer + (__instance.m_fadeOutDuration - __instance.m_fadeInDuration) * 0.1f;
        }
    }
}
