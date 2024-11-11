using Enemies;
using HarmonyLib;
using static Enemies.EnemyUpdateManager;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyTargetPatches
    {
        [HarmonyPatch(typeof(EnemyUpdateManager), nameof(EnemyUpdateManager.MasterMindUpdateFixed))]
        [HarmonyPostfix]
        private static void PostManagerUpdate(EnemyUpdateManager __instance)
        {
            ForceIntervalUpdate(__instance.m_detectionUpdatesClose);
            ForceIntervalUpdate(__instance.m_detectionUpdatesNear);
            ForceIntervalUpdate(__instance.m_detectionUpdatesFar);
        }

        private static void ForceIntervalUpdate(UpdateDetectionGroup group)
        {
            foreach (var behaviour in group.m_members)
                behaviour.m_ai.m_enemyAgent.m_validTargetInterval = 0;
        }
    }
}
