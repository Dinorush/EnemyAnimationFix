using HarmonyLib;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal class EnemyTagPatches
    {
        // Patching both so that, as host, you can fix the issue for non-modded clients,
        // but as client w/o modded host, you still have some fix (for some cases).
        [HarmonyPatch(typeof(ToolSyncManager), nameof(ToolSyncManager.DoTagEnemyValidation))]
        [HarmonyPrefix]
        private static bool Pre_DoTagEnemyValidation(ToolSyncManager.pTagEnemy data)
        {
            return data.enemy.TryGet(out var agent) && agent.Alive;
        }
        
        [HarmonyPatch(typeof(ToolSyncManager), nameof(ToolSyncManager.DoTagEnemy))]
        [HarmonyPrefix]
        private static bool Pre_DoTagEnemy(ToolSyncManager.pTagEnemy data)
        {
            return data.enemy.TryGet(out var agent) && agent.Alive;
        }
    }
}
