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
    }
}
