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

        [HarmonyPatch(typeof(EB_InCombat_MoveToNextNode_PathOpen), nameof(EB_InCombat_MoveToNextNode_PathOpen.UpdateBehaviour))]
        [HarmonyPostfix]
        private static void PathfindingFix(EB_InCombat_MoveToNextNode __instance)
        {
            if (__instance.m_machine.CurrentState.ENUM_ID != (byte)EB_States.InCombat && __instance.m_ai.IsTargetValid && __instance.m_ai.Target.m_nodeDistance <= 1)
                __instance.TryUpdateNavigation(EnemyCourseNavigationMode.MoveToNextNode);
        }

        [HarmonyPatch(typeof(EB_InCombatFlyer_GeomorphTraversal), nameof(EB_InCombatFlyer_GeomorphTraversal.Update))]
        [HarmonyPostfix]
        private static void FlyerPathfindingFix(EB_InCombat_MoveToNextNode __instance)
        {
            if (__instance.m_machine.CurrentState.ENUM_ID != (byte)EB_States.InCombat && __instance.m_ai.IsTargetValid && __instance.m_ai.Target.m_nodeDistance < 1)
                __instance.m_machine.ChangeState((byte)EB_States.InCombat);
        }

        [HarmonyPatch(typeof(EB_InCombat_MoveToNextNode_DestroyDoor), nameof(EB_InCombat_MoveToNextNode_DestroyDoor.UpdateBehaviour))]
        [HarmonyPrefix]
        private static void RemoteDoorFix(EB_InCombat_MoveToNextNode_DestroyDoor __instance, ref IntPtr __state)
        {
            var navigation = __instance.m_ai.m_courseNavigation;
            __state = navigation.IsPathBlocked ? navigation.m_navPortal.Pointer : IntPtr.Zero;
        }

        [HarmonyPatch(typeof(EB_InCombat_MoveToNextNode_DestroyDoor), nameof(EB_InCombat_MoveToNextNode_DestroyDoor.UpdateBehaviour))]
        [HarmonyPostfix]
        private static void RemoteDoorFixPost(EB_InCombat_MoveToNextNode_DestroyDoor __instance, IntPtr __state)
        {
            if (__state == IntPtr.Zero) return;

            var navigation = __instance.m_ai.m_courseNavigation;
            if (navigation.IsPathBlocked && navigation.m_navPortal.Pointer != __state)
                __instance.m_machine.ChangeState((byte)EB_States.InCombat_MoveToNextNode_PathBlocked);
        }
    }
}
