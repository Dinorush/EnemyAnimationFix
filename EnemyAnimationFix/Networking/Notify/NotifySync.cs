using SNetwork;

namespace EnemyAnimationFix.Networking.Notify
{
    internal sealed class NotifySync : SyncedEvent<NotifyData>
    {
        public override string GUID => "EAFNF";

        protected override void Receive(NotifyData packet)
        {
            if (SNet.TryGetPlayer(packet.lookup, out SNet_Player player))
                NotifyManager.ReceiveNotify(player);
        }
    }

    public struct NotifyData
    {
        public ulong lookup;
    }
}
