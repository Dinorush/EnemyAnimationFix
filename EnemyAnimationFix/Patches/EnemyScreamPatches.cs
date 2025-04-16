using Enemies;
using HarmonyLib;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyScreamPatches
    {
        [HarmonyPatch(typeof(ES_Scream), nameof(ES_Scream.Enter))]
        [HarmonyPostfix]
        private static void FixScreamReset(ES_Scream __instance)
        {
            __instance.m_hasTriggeredPropagation = false;
        }

        [HarmonyPatch(typeof(ES_Scream), nameof(ES_Scream.OnStateShouldBeActivated))]
        [HarmonyPrefix]
        private static void FixGiantAnim(ES_Scream __instance, ref pES_EnemyScreamData data)
        {
            if (__instance.m_locomotion.AnimHandleName != EnemyLocomotion.AnimatorControllerHandleName.EnemyGiant) return;

            data.AnimIndex = 0;
        }
    }
}
