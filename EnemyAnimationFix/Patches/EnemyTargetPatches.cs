using Enemies;
using HarmonyLib;
using System;
namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyTargetPatches
    {
        [HarmonyPatch(typeof(EnemyGroup), nameof(EnemyGroup.RegisterMember))]
        [HarmonyPostfix]
        private static void PostEnemyRegistered(EnemyGroup __instance, EnemyAgent enemyAgent)
        {
            switch (__instance.GroupType)
            {
                case EnemyGroupType.Survival:
                case EnemyGroupType.Hunters:
                    break;
                default:
                    return;
            }
            enemyAgent.m_hasValidTarget = false;
            enemyAgent.m_validTargetInterval = Clock.Time + UnityEngine.Random.RandomRange(Configuration.MinWaveSleepTime, Configuration.MaxWaveSleepTime);

            EnemyAgent.TakeDamageDelegate del = null!;
            del = (Action<float, ES_HitreactType>)((_, _) =>
            {
                enemyAgent.m_validTargetInterval = 0;
                enemyAgent.TookDamage -= del;
            });
            enemyAgent.TookDamage += del;
        }
    }
}
