using AIGraph;
using BepInEx.Unity.IL2CPP.Hook;
using EnemyAnimationFix.API;
using GTFO.API;
using Il2CppInterop.Runtime.Runtime;
using Player;
using PlayerCoverage;
using System;

namespace EnemyAnimationFix.NativePatches
{
    internal static class ClosestPlayerPatches
    {
        private static INativeDetour? TryGetClosestAliveDetour;
        private static d_TryGetClosestAlive? orig_TryGetClosestAlive;
        private unsafe delegate bool d_TryGetClosestAlive(IntPtr node, out IntPtr playerAgent, Il2CppMethodInfo* methodInfo);

        internal unsafe static void Apply()
        {
            TryGetClosestAliveDetour = INativeDetour.CreateAndApply(
                (nint)Il2CppAPI.GetIl2CppMethod<PlayerManager>(
                    nameof(PlayerManager.TryGetClosestAlivePlayerAgent),
                    typeof(bool).FullName,
                    false,
                    new[] {
                        nameof(AIG_CourseNode),
                        typeof(PlayerAgent).MakeByRefType().FullName
                    }),
                TryGetClosestAlivePatch,
                out orig_TryGetClosestAlive
                );

            NativePatchAPI.AddClosestAlivePrefix(FixSlotIndex);
        }

        private static unsafe bool TryGetClosestAlivePatch(IntPtr node, out IntPtr playerAgent, Il2CppMethodInfo* methodInfo)
        {
            playerAgent = IntPtr.Zero;
            PlayerAgent? player = null;
            bool result = false;
            AIG_CourseNode courseNode = new(node);

            bool runOriginal = true;
            try
            {
                runOriginal = NativePatchAPI.RunClosestAlivePrefix(courseNode, ref player, ref result);
            }
            catch (Exception e)
            {
                DinoLogger.Error($"Error running TryGetClosestAlivePrefix: {e.StackTrace}");
            }

            if (runOriginal)
            {
                result = orig_TryGetClosestAlive!(node, out playerAgent, methodInfo);
                player = playerAgent != IntPtr.Zero ? new(playerAgent) : null;
            }

            try
            {
                NativePatchAPI.RunClosestAlivePostfix(courseNode, ref player, ref result);
            }
            catch (Exception e)
            {
                DinoLogger.Error($"Error running TryGetClosestAlivePostfix: {e.StackTrace}");
            }

            playerAgent = player != null ? player.Pointer : IntPtr.Zero;
            return result;
        }

        private static bool FixSlotIndex(AIG_CourseNode courseNode, ref PlayerAgent? playerAgent, ref bool result)
        {
            PlayerAgent? fallbackAgent = null;
            bool foundPlayer = false;
            int lowestDist = int.MaxValue;

            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                if (player == null || !player.Alive || player.Dimension.IsArenaDimension) continue;

                fallbackAgent = player;
                var slot = player.PlayerSlotIndex;
                var coverageDatas = courseNode.m_playerCoverage.m_coverageDatas;
                if (slot < coverageDatas.Length)
                {
                    PlayerCoverageSystem.PlayerCoverageData playerCoverageData = coverageDatas[slot];
                    if (!playerCoverageData.IsValidNodeDistance) continue;

                    var nodeDist = playerCoverageData.m_nodeDistance;
                    if (!foundPlayer || nodeDist < lowestDist)
                    {
                        foundPlayer = true;
                        lowestDist = nodeDist;
                        playerAgent = player;
                    }
                }
            }

            if (!foundPlayer)
                playerAgent = fallbackAgent;

            result = playerAgent != null;
            return false;
        }
    }
}
