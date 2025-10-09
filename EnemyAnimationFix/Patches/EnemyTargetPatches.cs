using Enemies;
using HarmonyLib;
namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyTargetPatches
    {
        [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.RegisterMember))]
        [HarmonyPostfix]
        private static void PostEnemyRegistered(EnemyAgent enemyAgent)
        {
            enemyAgent.m_hasValidTarget = false;
            enemyAgent.m_validTargetInterval = Clock.Time + UnityEngine.Random.RandomRange(Configuration.MinWaveSleepTime, Configuration.MaxWaveSleepTime);
        }

        [HarmonyPatch(typeof(EB_InCombat_MoveToNextNode_PathOpen), nameof(EB_InCombat_MoveToNextNode_PathOpen.UpdateBehaviour))]
        [HarmonyPostfix]
        private static void PathfindingFix(EB_InCombat_MoveToNextNode __instance)
        {
            if (__instance.m_machine.CurrentState.ENUM_ID != (byte)EB_States.InCombat && __instance.m_ai.IsTargetValid && __instance.m_ai.Target.m_nodeDistance <= 1)
                __instance.TryUpdateNavigation(EnemyCourseNavigationMode.MoveToNextNode);
        }
    }
}
