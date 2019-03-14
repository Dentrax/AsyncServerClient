#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ServerFramework;

namespace ServerConsole.PacketProcessing
{
    interface IPacketReceiver
    {
        //Distribution to classes
        void OnCommandReceived(Packet packet);
    }
}
