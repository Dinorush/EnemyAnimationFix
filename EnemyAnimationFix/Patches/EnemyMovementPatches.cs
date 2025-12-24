using BepInEx.Unity.IL2CPP.Utils.Collections;
using Enemies;
using EnemyAnimationFix.Networking.Notify;
using EnemyAnimationFix.Utils.Extensions;
using HarmonyLib;
using SNetwork;
using StateMachines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyAnimationFix.Patches
{
    [HarmonyPatch]
    internal static class EnemyMovementPatches
    {
        private const float MinBufferTime = 0.25f;
        private static readonly Dictionary<IntPtr, float> _exitTimes = new();
        private static readonly Dictionary<IntPtr, Coroutine> _smoothRoutines = new();
        private static readonly HashSet<IntPtr> _usedFogs = new();

        public static void OnCleanup()
        {
            _exitTimes.Clear();
        }

        internal static bool Pre_ChangeState(StateMachine<ES_Base> __instance, ES_Base newState)
        {
            if (SNet.IsMaster) return true;

            if (newState.m_stateEnum != ES_StateEnum.PathMove) return true;
            var ownerPtr = newState.m_enemyAgent.Pointer;
            if (__instance.m_currentState.m_stateEnum == ES_StateEnum.TriggerFogSphere && _usedFogs.Remove(ownerPtr)) return true;
            if (Clock.Time >= _exitTimes.GetValueOrDefault(ownerPtr)) return true;

            // If the buffer is empty, state is synced normally
            ES_PathMove? pathMove = newState.TryCast<ES_PathMove>();
            if (pathMove == null || pathMove.m_positionBuffer.Count == 0) return true;

            StartUpdatePosition(pathMove.m_enemyAgent, pathMove);
            return false;
        }

        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.SetMaxSfxLOD))]
        [HarmonyPostfix]
        private static void Pre_Exit(EnemyAgent __instance)
        {
            pES_PathMoveData data;
            var pathMoveBase = __instance.Locomotion.PathMove;
            if (pathMoveBase.m_stateEnum != ES_StateEnum.PathMove) return;

            var pathMove = pathMoveBase.TryCast<ES_PathMove>();
            if (pathMove == null) return;

            var pointer = __instance.Pointer;
            if (!_exitTimes.ContainsKey(pointer))
                __instance.AddOnDeadOnce(() => _exitTimes.Remove(pointer));
            _exitTimes[__instance.Pointer] = Clock.Time + MinBufferTime;

            if (SNet.IsMaster) // Force send position data when leaving PathMove to sync clients' positions
            {
                data = pathMove.m_pathMoveData;
                data.CourseNode.Set(__instance.CourseNode);
                data.Position = __instance.m_position;
                data.Movement.Set(pathMove.m_moveDir, 1f);
                data.TargetLookDir.Value = __instance.TargetLookDir;
                ++data.Tick;
                pathMove.m_pathMoveData = data;
                foreach (var client in NotifyManager.FixedClients)
                    pathMove.m_pathMovePacket.Send(data, SNet_ChannelType.GameNonCritical, client);
                return;
            }

            if (pathMove.m_positionBuffer.Count == 0) return;

            // Force update position data when leaving PathMove to sync position
            StartUpdatePosition(__instance, pathMove);
        }

        private static void StartUpdatePosition(EnemyAgent enemy, ES_PathMove pathMove)
        {
            var data = pathMove.m_positionBuffer[^1];

            enemy.Locomotion.ForceNode(data.CourseNode);
            pathMove.m_targetPosition = data.Position;
            enemy.TargetLookDir = data.TargetLookDir.Value;
            pathMove.m_moveDir = data.Movement.Get(1f);

            if (!enemy.MovingCuller.IsShown || Configuration.SyncLerpTime <= 0f)
            {
                UpdatePosition(enemy, pathMove, data.Position);
                return;
            }

            if (_smoothRoutines.TryGetValue(enemy.Pointer, out var coroutine))
                enemy.StopCoroutine(coroutine);
            _smoothRoutines[enemy.Pointer] = enemy.StartCoroutine(LerpPosition(enemy, pathMove).WrapToIl2Cpp());
        }

        private static void UpdatePosition(EnemyAgent enemy, ES_PathMove pathMove, Vector3 pos)
        {
            enemy.transform.position = pos;
            pathMove.m_lastPos = pos;
            enemy.Position = pos;

            enemy.MovingCuller.UpdatePosition(enemy.DimensionIndex, pos);
            enemy.MovingCuller.Culler.NeedsShadowRefresh = true;
        }

        private static IEnumerator LerpPosition(EnemyAgent enemy, ES_PathMove pathMove)
        {
            var data = pathMove.m_positionBuffer[^1];
            Vector3 posDiff = data.Position - enemy.m_position;

            float lastTime = Clock.Time;
            float totalTime = 0f;
            float prevProgress = 0f;
            var locomotion = enemy.Locomotion;

            while (totalTime < Configuration.SyncLerpTime)
            {
                yield return null;
                if (locomotion.CurrentStateEnum == ES_StateEnum.PathMove)
                {
                    _smoothRoutines.Remove(enemy.Pointer);
                    yield break;
                }

                float time = Clock.Time;
                totalTime += time - lastTime;
                lastTime = time;
                float t = Math.Clamp(totalTime / Configuration.SyncLerpTime, 0f, 1f);

                float progress = 1f - (1f - t) * (1f - t);
                float delta = progress - prevProgress;
                prevProgress = progress;

                UpdatePosition(enemy, pathMove, enemy.m_position + posDiff * delta);
            }

            _smoothRoutines.Remove(enemy.Pointer);
        }

        [HarmonyPatch(typeof(EAB_FogSphere), nameof(EAB_FogSphere.DoTrigger))]
        [HarmonyPostfix]
        private static void Post_TriggerFog(EAB_FogSphere __instance)
        {
            _usedFogs.Add(__instance.m_owner.Pointer);
        }
    }
}
