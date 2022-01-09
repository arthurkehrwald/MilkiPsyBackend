using System;
using Newtonsoft.Json;

namespace MilkiPsyBackend
{ 
    // TODO: Create Outbound and Inbound packet classes
    public class ToClientPacket : Packet
    {
        public enum MessageType { Invalid, Feedback, Popup, ChangeStage };

        // TODO: Replace clientState arg with reference to state tracker
        public ToClientPacket(IMessageData packetData, ClientState clientState, bool ignoreMessageIfStateOutdated)
        {
            MessageType type = MessageTypeOf(packetData);

            if (type == MessageType.Invalid)
            {
                string nameOfTypeOfPacketData = packetData.GetType().ToString();
                throw new Exception($"[PacketGenerator] Failed to generate packet " +
                    $"because the type {nameOfTypeOfPacketData} is not associated" +
                    $"with any packet type");
            }

            MessageMetaData metaData = new()
            {
                type = type,
                currentState = clientState,
                ignoreMessageIfStateOutdated = ignoreMessageIfStateOutdated
            };

            string metaDataJson = JsonConvert.SerializeObject(metaData);
            string messageDataJson = JsonConvert.SerializeObject(packetData);
            Write(metaDataJson);
            Write(messageDataJson);
            WriteLength();
        }

        /// <summary>
        /// Associate each type of IPacketData with an enum value
        /// to be sent in the metadata so client knows how to parse
        /// the main message
        /// </summary>
        private static MessageType MessageTypeOf(IMessageData packetData)
        {
            if (packetData.GetType() == typeof(FeedbackMessageData))
            {
                return MessageType.Feedback;
            }

            if (packetData.GetType() == typeof(PopupMessageData))
            {
                return MessageType.Popup;
            }

            if (packetData.GetType() == typeof(ChangeStageMessageData))
            {
                return MessageType.ChangeStage;
            }

            return MessageType.Invalid;
        }

        private struct MessageMetaData
        {
            public MessageType type;
            public ClientState currentState;
            public bool ignoreMessageIfStateOutdated;
        }
    }

    public interface IMessageData { }

    public struct FeedbackMessageData : IMessageData
    {
        public string jsonFilename;
    }

    public struct PopupMessageData : IMessageData
    {
        public string jsonFileName;
    }

    public struct ChangeStageMessageData : IMessageData
    {
        public enum Function { None, Previous, Next, SetIndex };
        public Function function;
        public int index;
    }
}
