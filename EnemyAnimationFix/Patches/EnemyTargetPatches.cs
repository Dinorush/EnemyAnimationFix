using Enemies;
using HarmonyLib;
using static Enemies.EnemyUpdateManager;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyTargetPatches
    {
        private const float ValidTargetInterval = 1f;

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
            float time = Clock.Time;
            foreach (var behaviour in group.m_members)
            {
                var enemy = behaviour.m_ai.m_enemyAgent;
                if (enemy.m_validTargetInterval - time <= ValidTargetInterval)
                    behaviour.m_ai.m_enemyAgent.m_validTargetInterval = 0;
            }
        }

        [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.RegisterMember))]
        [HarmonyPostfix]
        private static void PostEnemyRegistered(EnemyAgent enemyAgent)
        {
            enemyAgent.m_hasValidTarget = false;
            enemyAgent.m_validTargetInterval = Clock.Time + UnityEngine.Random.RandomRange(Configuration.MinWaveSleepTime, Configuration.MaxWaveSleepTime) + ValidTargetInterval;
        }
    }
}
