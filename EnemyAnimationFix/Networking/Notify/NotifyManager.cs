﻿using SNetwork;
using System.Collections.Generic;

namespace EnemyAnimationFix.Networking.Notify
{
    internal static class NotifyManager
    {
        private readonly static NotifySync Sync = new();

        internal static void Init()
        {
            Sync.Setup();
        }

        public static bool MasterHasFix { get; private set; } = false;
        public static readonly List<SNet_Player> FixedClients = new();

        internal static void SendNotify(SNet_Player player)
        {
            Sync.Send(new() { lookup = SNet.LocalPlayer.Lookup }, player);
        }

        internal static void ReceiveNotify(SNet_Player player)
        {
            if (SNet.IsMaster)
                FixedClients.Add(player);
            else
                MasterHasFix = true;
        }

        internal static void SetMaster()
        {
            MasterHasFix = true;
        }

        internal static void Reset()
        {
            MasterHasFix = false;
            FixedClients.Clear();
        }

        internal static void RemoveClient(SNet_Player player)
        {
            for (int i = FixedClients.Count - 1; i >= 0; i--)
            {
                if (FixedClients[i] == null || FixedClients[i].Lookup == player.Lookup)
                    FixedClients.RemoveAt(i);
            }
        }
    }
}
