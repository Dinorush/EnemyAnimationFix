using Agents;
using Enemies;
using StateMachines;
using System.Collections.Generic;

namespace EnemyAnimationFix.API
{
    public static class NativePatchAPI
    {
        public delegate bool ChangeStatePrefix(StateMachine<ES_Base> __instance, ES_Base state);
        public delegate void ChangeStatePostfix(StateMachine<ES_Base> __instance, ES_Base state);

        private static readonly List<ChangeStatePrefix> s_changePrefix = new();
        private static readonly List<ChangeStatePostfix> s_changePostfix = new();

        public static void AddChangeStatePrefix(ChangeStatePrefix detectPrefix) => s_changePrefix.Add(detectPrefix);
        public static void AddChangeStatePostfix(ChangeStatePostfix detectPostfix) => s_changePostfix.Add(detectPostfix);

        internal static bool RunChangeStatePrefix(StateMachine<ES_Base> __instance, ES_Base state)
        {
            bool runOrig = true;
            foreach (var prefix in s_changePrefix)
                runOrig &= prefix(__instance, state);
            return runOrig;
        }

        internal static void RunChangeStatePostfix(StateMachine<ES_Base> __instance, ES_Base state)
        {
            foreach (var postFix in s_changePostfix)
                postFix(__instance, state);
        }
    }
}
