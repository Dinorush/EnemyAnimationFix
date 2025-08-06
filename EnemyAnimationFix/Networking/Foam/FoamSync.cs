using Agents;
using SNetwork;

namespace EnemyAnimationFix.Networking.Foam
{
    internal sealed class FoamSync : SyncedEvent<FoamData>
    {
        public override string GUID => "FOAM";

        protected override void Receive(FoamData packet)
        {
            if (packet.enemy.TryGet(out var enemy))
                FoamManager.ReceiveFoam(enemy, packet.foamRel.Get(1f));
        }
    }

    public struct FoamData
    {
        public pEnemyAgent enemy;
        public UFloat16 foamRel;
    }
}
