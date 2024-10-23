using Enemies;
using EnemyAnimationFix.Networking.Notify;
using HarmonyLib;
using SNetwork;
using StateMachines;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyMovementPatches
    {
        internal static bool Pre_ChangeState(StateMachine<ES_Base> _, ES_Base newState)
        {
            if (SNet.IsMaster || newState.m_stateEnum != ES_StateEnum.PathMove || Clock.Time - _exitTime > MinBufferTime) return true;

            ES_PathMove pathMove = newState.Cast<ES_PathMove>();
            if (pathMove.m_positionBuffer.Count == 0) return true;

            ForcePosition(pathMove.m_enemyAgent, pathMove, pathMove.m_positionBuffer[^1]);
            return false;
        }

        private const float MinBufferTime = 0.1f;
        private static float _exitTime = 0f;
        [HarmonyPatch(typeof(ES_PathMove), nameof(ES_PathMove.Exit))]
        [HarmonyPrefix]
        private static void Pre_Exit(ES_PathMove __instance)
        {
            _exitTime = Clock.Time;
            pES_PathMoveData data;
            EnemyAgent enemy = __instance.m_enemyAgent;

            if (SNet.IsMaster) // Force send position data when leaving PathMove to sync clients' positions
            {
                data = __instance.m_pathMoveData;
                data.CourseNode.Set(enemy.CourseNode);
                data.Position = enemy.Position;
                data.Movement.Set(__instance.m_moveDir, 1f);
                data.TargetLookDir.Value = enemy.TargetLookDir;
                data.Tick++;
                __instance.m_pathMoveData = data;
                foreach (var client in NotifyManager.FixedClients)
                    __instance.m_pathMovePacket.Send(data, SNet_ChannelType.GameNonCritical, client);
                return;
            }

            if (__instance.m_positionBuffer.Count == 0) return;

            // Force update position data when leaving PathMove to sync position
            data = __instance.m_positionBuffer[^1];
            ForcePosition(enemy, __instance, data);
        }

        private static void ForcePosition(EnemyAgent enemy, ES_PathMove pathMove, pES_PathMoveData data)
        {
            enemy.transform.position = enemy.Position;
            enemy.MovingCuller.UpdatePosition(enemy.DimensionIndex, enemy.Position);
            enemy.Position = data.Position;
            pathMove.m_lastPos = data.Position;
            enemy.Locomotion.ForceNode(data.CourseNode);
            pathMove.m_targetPosition = data.Position;
            pathMove.m_moveDir = data.Movement.Get(1f);
            enemy.TargetLookDir = data.TargetLookDir.Value;
            pathMove.UpdateRotation();
            pathMove.UpdateLocalAnimator();
            pathMove.UpdateAnimationBlend(pathMove.m_animFwd, pathMove.m_animRight);
        }
    }
}
