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
        private readonly static List<C_CullBucket> _nearbyCullers = new(NearbyCap);
        private readonly static Dictionary<IntPtr, (C_CullBucket bucket, float sqrDist)> _cachedCullers = new();
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

            var time = Clock.Time;
            if (time > _nextUpdateTime)
            {
                _nextUpdateTime = time + UpdateInterval;
                CacheCullers();
            }

            foreach (var culler in _nearbyCullers)
            {
                if (culler.CullKey != C_Keys.CurrentCullKey)
                {
                    culler.CullKey = C_Keys.CurrentCullKey;
                    culler.Show();
                }
            }
        }

        private static void CacheCullers()
        {
            _nearbyCullers.Clear();
            if (!PlayerManager.HasLocalPlayerAgent()) return;

            var player = PlayerManager.GetLocalPlayerAgent();
            var pos = player.Position;
            var node = player.m_movingCuller.CurrentNode;
            CacheNearbyEnemies(node, pos);
            if (_cachedCullers.Count < NearbyCap)
            {
                foreach (var portal in node.m_portals)
                {
                    if (portal.IsOpen && (portal.Bounds.ClosestPoint(pos) - pos).sqrMagnitude < NearbySqrDist)
                        CacheNearbyEnemies(portal.GetOpposite(node), pos);

                    if (_cachedCullers.Count > NearbyCap) break;
                }
            }

            _nearbyCullers.AddRange(
                _cachedCullers
                .OrderBy(kv => kv.Value.sqrDist)
                .Take(NearbyCap)
                .Select(kv => kv.Value.bucket)
                );
            _cachedCullers.Clear();
        }

        private static void CacheNearbyEnemies(C_Node node, Vector3 pos)
        {
            foreach (var culler in node.m_movingCullers)
            {
                var bucket = culler.CullBucket;
                if (_cachedCullers.ContainsKey(bucket.Pointer)) continue;

                var sqrDiff = (culler.m_position - pos).sqrMagnitude;
                if (sqrDiff < NearbySqrDist)
                    _cachedCullers.Add(bucket.Pointer, (bucket, sqrDiff));

                if (_cachedCullers.Count > NearbyCap) return;
            }
        }

        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.SetAnimatorCullingEnabled))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void FixFootstepCulling(EnemyAgent __instance, bool mode)
        {
            if (!Configuration.DisableNearCull || !mode) return;

            __instance.Locomotion.m_animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.DoStartMeleeAttack))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static void DisableMeleeAnimCulling(ES_StrikerMelee __instance)
        {
            if (!Configuration.DisableNearCull) return;

            __instance.m_locomotion.m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.Exit))]
        [HarmonyPatch(typeof(ES_StrikerMelee), nameof(ES_StrikerMelee.SyncExit))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void EnableMeleeAnimCulling(ES_StrikerMelee __instance)
        {
            if (!Configuration.DisableNearCull) return;

            var agent = __instance.m_locomotion.m_agent;
            agent.SetAnimatorCullingEnabled(agent.MovingCuller.m_animatorCullingEnabled);
        }
    }
}
