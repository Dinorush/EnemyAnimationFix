using Enemies;
using HarmonyLib;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyDeathPatches
    {
        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.Alive), MethodType.Setter)]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void Post_Death(EnemyAgent __instance, bool value)
        {
            if (value || __instance == null || __instance.UpdateMode != NodeUpdateMode.None) return;

            EnemyUpdateManager.Current.Register(__instance, NodeUpdateMode.Close);
        }
    }
}
