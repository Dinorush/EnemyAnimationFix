using BepInEx.Unity.IL2CPP.Hook;
using EnemyAnimationFix.API;
using Enemies;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.MethodInfo;
using StateMachines;
using System;
using System.Runtime.InteropServices;
using EnemyAnimationFix.Networking.Notify;
using EnemyAnimationFix.Utils;

namespace EnemyAnimationFix.NativePatches
{
    internal static class ChangeStatePatches
    {
        private static INativeDetour? ChangeStateDetour;
        private static d_ChangeStateFromQueue? orig_ChangeStateFromQueue;
        private unsafe delegate void d_ChangeStateFromQueue(IntPtr _this, Il2CppMethodInfo* methodInfo);

        // Can't harmony patch the function due to out parameter so need a native detour
        internal unsafe static void ApplyNativePatch()
        {
            NativePatchAPI.AddChangeStatePrefix(FixMeleeCancel);

            INativeClassStruct val = UnityVersionHandler.Wrap((Il2CppClass*) Il2CppClassPointerStore<StateMachine<ES_Base>>.NativeClassPtr);

            for (int i = 0; i < val.MethodCount; i++)
            {
                INativeMethodInfoStruct val2 = UnityVersionHandler.Wrap(val.Methods[i]);

                if (Marshal.PtrToStringAnsi(val2.Name) == "ChangeStateFromQueue")
                {
                    ChangeStateDetour = INativeDetour.CreateAndApply<d_ChangeStateFromQueue>(val2.MethodPointer, ChangeStatePatch, out orig_ChangeStateFromQueue);
                    return;
                }
            }
        }

        private unsafe static void ChangeStatePatch(IntPtr _this, Il2CppMethodInfo* methodInfo)
        {
            StateMachine<ES_Base> machine = new(_this);
            if (machine.CurrentState == null)
            {
                orig_ChangeStateFromQueue!(_this, methodInfo);
                return;
            }

            ES_Base state = machine.m_stateQueue.Peek();

            bool runOriginal = true;
            try
            {
                runOriginal = NativePatchAPI.RunChangeStatePrefix(machine, state);
            }
            catch (Exception e)
            {
                DinoLogger.Error($"Error running ChangeStatePrefix: {e.StackTrace}");
            }

            if (runOriginal)
                orig_ChangeStateFromQueue!(_this, methodInfo);
            else
                machine.m_stateQueue.Dequeue();

            try
            {
                NativePatchAPI.RunChangeStatePostfix(machine, state);
            }
            catch (Exception e)
            {
                DinoLogger.Error($"Error running ChangeStatePostfix: {e.StackTrace}");
            }
        }

        private static bool FixMeleeCancel(StateMachine<ES_Base> __instance, ES_Base newState)
        {
            // Fix enemies canceling their melee when a player is inside them
            ES_Base state = __instance.CurrentState;
            if (__instance.CurrentState == null || !NotifyManager.MasterHasFix || newState.m_stateEnum != ES_StateEnum.PathMove || state.m_stateEnum != ES_StateEnum.StrikerMelee) return true;

            ES_StrikerMelee melee = state.Cast<ES_StrikerMelee>();
            return (Clock.Time - melee.m_startTime) * melee.m_animSpeed >= melee.m_attackData.Duration;
        }
    }
}
