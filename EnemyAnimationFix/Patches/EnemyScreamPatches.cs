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
    }
}
