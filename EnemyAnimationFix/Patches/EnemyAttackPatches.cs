using Enemies;
using EnemyAnimationFix.Networking.Notify;
using HarmonyLib;
using System.Diagnostics.CodeAnalysis;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyAttackPatches
    {
        [HarmonyPatch(typeof(ES_StrikerAttack), nameof(ES_StrikerAttack.OnAttackPerform))]
        [HarmonyPostfix]
        private static void Post_StrikerAttackPerform(ES_StrikerAttack __instance)
        {
            if (!TryGetTentacle(__instance, out var tentacle)) return;

            // Fix tentacle attack animation ending early
            tentacle.m_inAttackMove = true;
        }

        [HarmonyPatch(typeof(ES_ShooterAttack), nameof(ES_ShooterAttack.OnAttackPerform))]
        [HarmonyPostfix]
        private static void Post_ShooterAttackPerform(ES_ShooterAttack __instance)
        {
            if (__instance.m_attackDoneTimer >= Clock.Time) return;

            // Fix shooter attack animation ending early
            EAB_ProjectileShooter ability = __instance.m_projectileAbility;
            __instance.m_attackDoneTimer = Clock.Time + (ability.m_shotDelayMin + ability.m_shotDelayMax) / 2f * ability.m_burstCount * 0.7f + __instance.m_burstCoolDownBeforeExit;
        }

        [HarmonyPatch(typeof(ES_StrikerAttack), nameof(ES_StrikerAttack.CommonExit))]
        [HarmonyPostfix]
        private static void Post_StrikerAttackExit(ES_StrikerAttack __instance)
        {
            if (!NotifyManager.MasterHasFix || Clock.Time <= __instance.m_performAttackTimer) return;

            if (!TryGetTentacle(__instance, out var tentacle) || tentacle.m_currentRoutine == null) return;

            tentacle.SwitchCoroutine(tentacle.AttackIn(tentacle.m_attackInDuration));
        }

        // Generally shouldn't fail, but can in some rare cases (idk why)
        private static bool TryGetTentacle(ES_StrikerAttack attack, [MaybeNullWhen(false)] out MovingEnemyTentacleBase tentacle)
        {
            tentacle = null;
            var ability = attack.m_tentacleAbility;
            if (ability == null) return false;

            tentacle = ability.m_tentacle;
            return tentacle != null;
        }

        [HarmonyPatch(typeof(ES_PathMove), nameof(ES_PathMove.Exit))]
        [HarmonyPostfix]
        private static void Post_TankPathMoveExit(ES_PathMove __instance)
        {
            if (!NotifyManager.MasterHasFix) return;

            // Fix omni-state for tank tentacles
            EnemyAbilities abilities = __instance.m_enemyAgent.Abilities;
            if (!ResetTankTongues(abilities, Agents.AgentAbility.Ranged))
                ResetTankTongues(abilities, Agents.AgentAbility.Melee);
        }

        private static bool ResetTankTongues(EnemyAbilities abilities, Agents.AgentAbility abilityType)
        {
            EnemyAbility? ability = abilities.GetAbility(abilityType);
            if (ability == null) return false;

            EAB_MovingEnemyTentacleMultiple? tankAbility = ability.TryCast<EAB_MovingEnemyTentacleMultiple>();
            if (tankAbility == null) return false;

            foreach (MovingEnemyTentacleBase tentacle in tankAbility.m_tentacles)
            {
                if (tentacle.m_currentRoutine == null) continue;

                tentacle.SwitchCoroutine(tentacle.AttackIn(tentacle.m_attackInDuration));
            }
            return true;
        }

        [HarmonyPatch(typeof(ES_ShooterAttack), nameof(ES_ShooterAttack.CommonExit))]
        [HarmonyPostfix]
        private static void Post_ShooterAttackExit(ES_ShooterAttack __instance)
        {
            if (!NotifyManager.MasterHasFix || Clock.Time <= __instance.m_performAttackTimer) return;

            __instance.m_projectileAbility.FireEnd();
        }
    }
}
