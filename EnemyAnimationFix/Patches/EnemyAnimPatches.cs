using Enemies;
using HarmonyLib;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyAnimPatches
    {
        [HarmonyPatch(typeof(ES_Scream), nameof(ES_Scream.OnStateShouldBeActivated))]
        [HarmonyPrefix]
        private static void FixGiantScream(ES_Scream __instance, ref pES_EnemyScreamData data)
        {
            if (__instance.m_locomotion.AnimHandleName != EnemyLocomotion.AnimatorControllerHandleName.EnemyGiant) return;

            data.AnimIndex = 0;
        }

        [HarmonyPatch(typeof(ES_StrikerAttack), nameof(ES_StrikerAttack.OnAttackWindUp))]
        [HarmonyPatch(typeof(ES_ShooterAttack), nameof(ES_ShooterAttack.OnAttackWindUp))]
        [HarmonyPrefix]
        private static void FixFiddlerAttack(ES_EnemyAttackBase __instance, ref int attackIndex)
        {
            if (__instance.m_locomotion.AnimHandleName != EnemyLocomotion.AnimatorControllerHandleName.EnemyFiddler) return;

            attackIndex %= 2;
        }

        [HarmonyPatch(typeof(ES_Hitreact), nameof(ES_Hitreact.DoHitReact))]
        [HarmonyPrefix]
        private static void FixLowStagger(ES_Hitreact __instance, ref int index, ES_HitreactType hitreactType, ImpactDirection impactDirection)
        {
            if (__instance.m_locomotion.AnimHandleName != EnemyLocomotion.AnimatorControllerHandleName.EnemyLow || hitreactType != ES_HitreactType.Light || impactDirection != ImpactDirection.Back) return;

            index %= 3;
        }
    }
}
