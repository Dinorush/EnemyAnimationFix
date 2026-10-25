using Agents;
using Enemies;
using OptionalNetworking;
using SNetwork;
using System.Collections.Generic;

namespace EnemyAnimationFix.Networking
{
    internal static class NetworkManager
    {
        private static ModInfo _modInfo = null!;
        private static CustomDataSender<FoamData> _foamSender = null!;

        public static IReadOnlyCollection<SNet_Player> FixedClients => _modInfo.OtherPlayers;
        public static bool MasterHasFix => _modInfo.MasterHasMod;

        internal static void Init()
        {
            _modInfo = OptionalNetworkAPI.RegisterMod(EntryPoint.MODNAME);
            _foamSender = _modInfo.AddCustomDataSender<FoamData>(ReceiveFoam);
        }

        public static void SendFoam(EnemyAgent enemy, float foamRel)
        {
            FoamData data = default;
            data.enemy.Set(enemy);
            data.foamRel.Set(foamRel, 1f);
            _foamSender.SendData(data);
        }

        private static void ReceiveFoam(SNet_Player player, FoamData data)
        {
            if (data.enemy.TryGet(out var enemy))
                enemy.Damage.m_attachedGlueVolume = data.foamRel.Get(1f) * enemy.EnemyBalancingData.GlueTolerance;
        }

        public struct FoamData
        {
            public pEnemyAgent enemy;
            public UFloat16 foamRel;
        }
    }
}
