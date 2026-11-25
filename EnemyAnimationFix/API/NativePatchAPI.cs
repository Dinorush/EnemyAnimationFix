using AIGraph;
using Enemies;
using Player;
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
            foreach (var postfix in s_changePostfix)
                postfix(__instance, state);
        }


        public delegate bool TryGetClosestAlivePrefix(AIG_CourseNode node, ref PlayerAgent? playerAgent, ref bool result);
        public delegate void TryGetClosestAlivePostfix(AIG_CourseNode node, ref PlayerAgent? playerAgent, ref bool result);

        private static readonly List<TryGetClosestAlivePrefix> s_closestAlivePrefix = new();
        private static readonly List<TryGetClosestAlivePostfix> s_closestAlivePostfix = new();

        public static void AddClosestAlivePrefix(TryGetClosestAlivePrefix closestAlivePrefix) => s_closestAlivePrefix.Add(closestAlivePrefix);
        public static void AddClosestAlivePostfix(TryGetClosestAlivePostfix closestAlivePostfix) => s_closestAlivePostfix.Add(closestAlivePostfix);

        internal static bool RunClosestAlivePrefix(AIG_CourseNode node, ref PlayerAgent? playerAgent, ref bool result)
        {
            bool runOrig = true;
            foreach (var prefix in s_closestAlivePrefix)
                runOrig &= prefix(node, ref playerAgent, ref result);
            return runOrig;
        }

        internal static void RunClosestAlivePostfix(AIG_CourseNode node, ref PlayerAgent? playerAgent, ref bool result)
        {
            foreach (var postfix in s_closestAlivePostfix)
                postfix(node, ref playerAgent, ref result);
        }
    }
}
