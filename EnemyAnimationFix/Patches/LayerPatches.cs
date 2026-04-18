using HarmonyLib;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class LayerPatches
    {
        [HarmonyPatch(typeof(LayerManager), nameof(LayerManager.Setup))]
        [HarmonyPostfix]
        private static void Post_Setup()
        {
            if (Configuration.DisableCorpseHitbox)
                LayerManager.MASK_MELEE_ATTACK_TARGETS_WITH_STATIC &= ~LayerManager.MASK_ENEMY_DEAD;
            else
                LayerManager.MASK_MELEE_ATTACK_TARGETS_WITH_STATIC |= LayerManager.MASK_ENEMY_DEAD;
        }
    }
}
