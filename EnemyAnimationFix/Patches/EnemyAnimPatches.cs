using Enemies;
using EnemyAnimationFix.Utils;
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

            attackIndex = UnityEngine.Random.Range(0, 2);
        }
    }
}
