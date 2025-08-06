using Enemies;
using EnemyAnimationFix.Networking.Notify;

namespace EnemyAnimationFix.Networking.Foam
{
    internal static class FoamManager
    {
        private readonly static FoamSync _sync = new();

        internal static void Init()
        {
            _sync.Setup();
        }

        internal static void SendFoam(EnemyAgent enemy, float foamRel)
        {
            FoamData data = default;
            data.enemy.Set(enemy);
            data.foamRel.Set(foamRel, 1f);

            foreach (var player in NotifyManager.FixedClients)
                _sync.Send(data, player);
        }

        internal static void ReceiveFoam(EnemyAgent enemy, float foamRel)
        {
            enemy.Damage.m_attachedGlueVolume = foamRel * enemy.EnemyBalancingData.GlueTolerance;
        }
    }
}
