﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public static class DO_RMCRequestMessage
    {
        enum DOC_METHOD
        {
            AddDuplicaLocation = 1,
            DeleteDuplica = 2,
            RemoveFromCachedDuplicationSet = 3,
            AdjustTime = 4,
            SyncRequest = 5,
            SyncResponse = 6,
            AskForSettingPlayerParameters = 7,
            AskForSettingPlayerState = 8,
            SetPlayerParameters = 9,
            SetPlayerState = 10,
            SetPlayerIdentity = 11,
            SetPlayerRDVInfo = 12,
            AskForSettingSessionParameters = 13,
            Disconnect = 14,
            IncreasePlayerNb = 15,
            OnEndMatch = 16,
            OnStartMatch = 17,
            PlayVoiceChat = 18,
            PlayVoiceChatWithMutedPlayers = 19,
            UpdateSessionHostAfterMigration = 20,
            RequestIDRangeFromMaster = 21,
            ConfirmElection = 22,
            DeclinePromotion = 23,
            ElectNewMaster = 24,
            KickOut = 25,
            ReportFault = 26,
            RetrieveURLs = 27,
            SynchronizeTermination = 28,
            SignalAsFaulty = 29,
            ProcessMessage = 30,
            RouteMessage = 31
        }

        public static byte[] HandleMessage(ClientInfo client, byte[] data)
        {
            Log.WriteLine(1, "[DO] Handling DO_RMCRequestMessage...");
            MemoryStream m = new MemoryStream(data);
            m.Seek(1, 0);
            ushort callID = Helper.ReadU16(m);
            uint flags = Helper.ReadU32(m);
            uint station = Helper.ReadU32(m);
            uint targetObject = Helper.ReadU32(m);
            DOC_METHOD method = (DOC_METHOD)Helper.ReadU16(m);
            Log.WriteLine(2, "[DO] RMC Call ID      : 0x" + callID.ToString("X4"));
            Log.WriteLine(2, "[DO] RMC Call Flags   : 0x" + flags.ToString("X8"));
            Log.WriteLine(2, "[DO] RMC Call Station : 0x" + station.ToString("X8"));
            Log.WriteLine(2, "[DO] RMC Call DupObj  : 0x" + targetObject.ToString("X8") + " " + new DupObj(targetObject).getDesc());
            switch (method)
            {
                case DOC_METHOD.SyncRequest:
                    Log.WriteLine(1, "[DO] Handling SyncRequest...");
                    ulong time = Helper.ReadU64(m);
                    return Create(callID, 0x83C, new DupObj(DupObjClass.Station, 1), new DupObj(DupObjClass.SessionClock, 1), 6, new Payload_SyncResponse(time).toBuffer());
                case DOC_METHOD.RequestIDRangeFromMaster:
                    Log.WriteLine(1, "[DO] Handling RequestIDRangeFromMaster...");
                    return DO_RMCResponseMessage.Create(callID, 0x60001, new byte[] { 0x01, 0x01, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00 });
                case DOC_METHOD.IncreasePlayerNb:
                    Log.WriteLine(1, "[DO] Handling IncreasePlayerNb...");
                    return DO_RMCResponseMessage.Create(callID, 0x60001, new byte[] { 0x00 });
                case DOC_METHOD.AskForSettingPlayerParameters:
                    Log.WriteLine(1, "[DO] Handling AskForSettingPlayerParameters...");
                    return DO_RMCResponseMessage.Create(callID, 0x60001, new byte[] { 0x00 });
                default:
                    Log.WriteLine(1, "[DO] Error: Unhandled DOC method: " + method + "!");
                    return new byte[0];
            }
        }

        public static byte[] Create(ushort callID, uint flags, uint station, uint target, ushort method, byte[] payload)
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU8(m, 0xA);
            Helper.WriteU16(m, callID);
            Helper.WriteU32(m, flags);
            Helper.WriteU32(m, station);
            Helper.WriteU32(m, target);
            Helper.WriteU16(m, method);
            m.Write(payload, 0, payload.Length);
            return m.ToArray();
        }
    }
}
