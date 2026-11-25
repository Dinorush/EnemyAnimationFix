using AIGraph;
using HarmonyLib;
using Player;
using System;
using UnityEngine;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class SurvivalWavePatches
    {
        [HarmonyPatch(typeof(SurvivalWave), nameof(SurvivalWave.GetScoredSpawnPoint_DirectionalWeight))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static bool FixDirectionalSpawns(SurvivalWave __instance, Vector3 dir, float maxDistance, ref SurvivalWave.ScoredSpawnPoint __result)
        {
            AIG_CourseNode courseNode = __instance.m_courseNode;
            if (PlayerManager.TryGetClosestAlivePlayerAgent(courseNode, out var playerAgent))
            {
                var playerNode = playerAgent.CourseNode;
                if (playerNode != null && playerNode.IsValid)
                    courseNode = playerNode;
            }
            var availableSpawnPoints = __instance.GetAvailableSpawnPoints(courseNode);
            SurvivalWave.ScoredSpawnPoint? bestScoredSpawnPoint = null;
            float bestScore = float.MinValue;

            // This function gets called with maxDistance = m_areaDistance + 2.
            // The usage of the min/max distance variables is so flawed, I'm just replacing the logic.
            float targetDistance = Math.Max(maxDistance - 2, 0);
            Vector3 position = courseNode.Position;
            foreach (var point in availableSpawnPoints)
            {
                Vector3 vector = point.firstCoursePortal.Position - position;
                vector.y = 0f;
                vector.Normalize();
                point.m_dir = vector;

                var nodeDistance = point.courseNode.m_playerCoverage.GetNodeDistanceToClosestPlayer();
                var dirScore = Math.Clamp(Vector3.Dot(vector, dir), 0, 1);
                var distScore = targetDistance - Math.Abs(targetDistance - nodeDistance);
                var score = 2f * distScore + dirScore + UnityEngine.Random.Range(0f, 0.1f);
                if (score > bestScore)
                {
                    bestScoredSpawnPoint = point;
                    point.totalCost = bestScore = score;
                }
            }

            if (bestScore == float.MinValue)
            {
                bestScoredSpawnPoint = new()
                {
                    courseNode = courseNode
                };
                Debug.LogError(" > Failed finding a good spawn node, using the center node instead!");
            }

            __result = bestScoredSpawnPoint!;
            return false;
        }
    }
}