using AIGraph;
using CullingSystem;
using Enemies;
using HarmonyLib;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal class EnemyCullPatches
    {
        private readonly static List<(Animator animator, C_CullBucket bucket)> _nearbyCullers = new(NearbyCap);
        private readonly static Dictionary<IntPtr, (EnemyAgent enemy, float sqrDist)> _cachedEnemies = new();
        private static float _nextUpdateTime = 0f;
        private const float UpdateInterval = 0.1f;
        private const float NearbySqrDist = 25f * 25f; // Seems near the high-end of footstep ranges (based on BBCs).
        private const int NearbyCap = 5; // Footstep audio seems to be capped by this amount anyway.

        [HarmonyPatch(typeof(C_Camera), nameof(C_Camera.PropagateFrustra))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void RestoreFootstepCullers()
        {
            if (!Configuration.DisableNearCull) return;

            CheckDeadCullers();

            var time = Clock.Time;
            if (time > _nextUpdateTime)
            {
                _nextUpdateTime = time + UpdateInterval;
                CacheCullers();
            }

            foreach ((_, var culler) in _nearbyCullers)
            {
                if (culler.CullKey != C_Keys.CurrentCullKey)
                {
                    culler.CullKey = C_Keys.CurrentCullKey;
                    culler.Show();
                }
            }
        }

        private static void CheckDeadCullers()
        {
            for (int i = _nearbyCullers.Count - 1; i >= 0; i--)
            {
                if (_nearbyCullers[i].animator == null)
                    _nearbyCullers.RemoveAt(i);
            }
        }

        private static void CacheCullers()
        {
            foreach ((var animator, _) in _nearbyCullers)
                if (animator.cullingMode == AnimatorCullingMode.CullUpdateTransforms)
                    animator.cullingMode = AnimatorCullingMode.CullCompletely;
            _nearbyCullers.Clear();
            if (!PlayerManager.HasLocalPlayerAgent()) return;

            var player = PlayerManager.GetLocalPlayerAgent();
            var pos = player.Position;
            var node = player.CourseNode;
            CacheNearbyEnemies(node, pos);
            if (_cachedEnemies.Count < NearbyCap)
            {
                foreach (var portal in node.m_portals)
                {
                    if (portal.IsTraversable && (portal.m_cullPortal.Bounds.ClosestPoint(pos) - pos).sqrMagnitude < NearbySqrDist)
                        CacheNearbyEnemies(portal.GetOppositeNode(node), pos);

                    if (_cachedEnemies.Count > NearbyCap) break;
                }
            }

            _nearbyCullers.AddRange(
                _cachedEnemies
                .OrderBy(kv => kv.Value.sqrDist)
                .Take(NearbyCap)
                .Select(kv => (kv.Value.enemy.Locomotion.m_animator, kv.Value.enemy.MovingCuller.CullBucket))
                );
            foreach ((var animator, _) in _nearbyCullers)
                if (animator.cullingMode == AnimatorCullingMode.CullCompletely)
                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            _cachedEnemies.Clear();
        }

        private static void CacheNearbyEnemies(AIG_CourseNode node, Vector3 pos)
        {
            foreach (var enemy in node.m_enemiesInNode)
            {
                if (_cachedEnemies.ContainsKey(enemy.Pointer)) continue;

                var sqrDiff = (enemy.Position - pos).sqrMagnitude;
                if (sqrDiff < NearbySqrDist)
                    _cachedEnemies.Add(enemy.Pointer, (enemy, sqrDiff));

                if (_cachedEnemies.Count > NearbyCap) return;
            }
        }

        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.SetAnimatorCullingEnabled))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void FixFootstepCulling(EnemyAgent __instance, bool mode)
        {
            if (!Configuration.DisableNearCull || !mode) return;
            
            var animator = __instance.Locomotion.m_animator;
            if (!_nearbyCullers.Any(pair => animator.Pointer == pair.animator.Pointer)) return;

            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        [HarmonyPatch(typeof(ES_EnemyAttackBase), nameof(ES_EnemyAttackBase.DoStartAttack))]
        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.DoStartMeleeAttack))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void DisableAttackAnimCulling(ES_EnemyAttackBase __instance)
        {
            if (!Configuration.DisableNearCull) return;

            __instance.m_locomotion.m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        [HarmonyPatch(typeof(ES_EnemyAttackBase), nameof(ES_EnemyAttackBase.CommonExit))]
        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.Exit))]
        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.SyncExit))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void EnableAttackCulling(ES_EnemyAttackBase __instance)
        {
            if (!Configuration.DisableNearCull) return;

            var agent = __instance.m_locomotion.m_agent;
            agent.SetAnimatorCullingEnabled(agent.MovingCuller.m_animatorCullingEnabled);
        }

        [HarmonyPatch(typeof(ES_Hitreact), nameof(ES_Hitreact.DoHitReact))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void DisableStaggerCulling(ES_HitreactBase __instance)
        {
            if (!Configuration.DisableNearCull) return;

            __instance.m_locomotion.m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        [HarmonyPatch(typeof(ES_Hitreact), nameof(ES_Hitreact.Exit))]
        [HarmonyPatch(typeof(ES_Hitreact), nameof(ES_Hitreact.SyncExit))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void EnableStaggerCulling(ES_HitreactBase __instance)
        {
            if (!Configuration.DisableNearCull) return;

            var agent = __instance.m_locomotion.m_agent;
            agent.SetAnimatorCullingEnabled(agent.MovingCuller.m_animatorCullingEnabled);
        }
    }
}
