using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Domain
{
    public class Peer
    {

        event Action<Peer> FileDownloadAction;

        private readonly Action<Peer> OnClose;
        event Action<Message> onMessage;
        private Func<string, Guid, RTCIceCandidateInit, Task> onIceCandidate;


        public Queue<RTCIceCandidateInit> iceQueue = new();
        public Peer(string name,
            Guid Id = default, Func<string, Guid, RTCIceCandidateInit, Task> onIceCandidate = null, Action<Peer> downloadFile = null, Action<Peer> OnClose = null, Action<Message> onMessage = null)
        {
            Name = name;
            RTCPeerConnection = new RTCPeerConnection();
            PeerConnectionId = Id != default ? Id : Guid.NewGuid();
            RTCPeerConnection.OnDataChannel += RegisterDC;
            this.OnClose = OnClose;
            this.onMessage += onMessage;
            this.onIceCandidate = onIceCandidate;
            RTCPeerConnection.OnIceCandidate += async (s, e) => await AddIceCandidateAsync(s, e);
            FileDownloadAction += downloadFile;
        }

        public RTCPeerConnection RTCPeerConnection { get; set; }
        public Guid PeerConnectionId { get; set; }

        public RTCDataChannel RTCDataChannel { get; set; }

        public string Name { get; set; }

        public DataChannelPayloadType CurrentPayloadType { get; set; }

        public int ChunksReceived { get; set; }

        FileMetadata _currentFileMetadata;
        public FileMetadata CurrentFileMetadata
        {
            get
            {
                return _currentFileMetadata;
            }
            set
            {
                _currentFileMetadata = value;
                ChunksReceived = 0;
            }
        }

        public List<byte[]> FileBuffer { get; set; } = new List<byte[]>();

        public float Progress { get { return ChunksReceived / CurrentFileMetadata.TotalChunks; } }

        public void SendFileChunk(byte[] chunk)
        {
            if (RTCDataChannel.ReadyState == RTCDataChannelState.Open)
            {
                RTCDataChannel.Send(chunk);

            }
        }
        private void RegisterDC(object sender, RTCDataChannel dc)
        {
            dc.OnMessage += ProcessMessage;
            dc.OnDataMessage += ProcessData;
            dc.OnClose += (s, e) => OnClose?.Invoke(this);
            dc.OnOpen += (s, e) => Console.WriteLine("its open!");
            Console.WriteLine(dc.ReadyState);
            RTCDataChannel = dc;
        }

        public void SendFileMetadata(FileMetadata metadata)
        {
            if (RTCDataChannel.ReadyState == RTCDataChannelState.Open)
            {
                RTCDataChannel.Send(DataChannelPayloadType.Metadata.ToString());
                RTCDataChannel.Send(metadata.Serialize());
            }
        }


        void ProcessMessage(object sender, string e)
        {
            if (Enum.TryParse(e, out DataChannelPayloadType result))
            {
                CurrentPayloadType = result;
            }
            else if (e != "ack")
            {
                Console.WriteLine(e);
                onMessage?.Invoke(new Message(e, Name));
            }
            return;

        }

        public void SendMessage(string message)
        {
            if (RTCDataChannel.ReadyState == RTCDataChannelState.Open)
            {
                RTCDataChannel.Send(message);
            }
        }
        void ProcessData(object sender, byte[] e)
        {

            if (CurrentPayloadType == DataChannelPayloadType.Metadata)
            {

                CurrentPayloadType = DataChannelPayloadType.File;
                CurrentFileMetadata = FileMetadata.Desserialize(e);
                SendMessage("ack");
                return;
            }
            if (CurrentPayloadType == DataChannelPayloadType.File)
            {

                FileBuffer.Add(e);
                ChunksReceived++;
                if (ChunksReceived == CurrentFileMetadata.TotalChunks)
                {
                    Console.WriteLine("downloading file");
                    FileDownloadAction?.Invoke(this);
                }
                return;
            }

        }


        public void CreateDataChannel(string channel)
        {
            RTCDataChannel = RTCPeerConnection.createDataChannel(channel);
            RTCDataChannel.OnOpen += (s, e) => Console.WriteLine("its open!");
            RTCDataChannel.OnClose += (s, e) => OnClose.Invoke(this);
            RTCDataChannel.OnMessage += ProcessMessage;
            RTCDataChannel.OnDataMessage += ProcessData;


        }

        public void AddMessageListener(Action<string> act)
        {
            RTCDataChannel.OnMessage += (s, e) => act(e);
        }

        public async Task TrickleIceAsync()
        {
            while (iceQueue.Count > 0)
            {
                var ice = iceQueue.Dequeue();
                await onIceCandidate(Name, PeerConnectionId, ice);
            }

        }

        async Task AddIceCandidateAsync(object sender, RTCIceCandidateInit iceCandidateInit)
        {

            if (Name != null)
            {
                await onIceCandidate(Name, PeerConnectionId, iceCandidateInit);
                while (iceQueue.Count > 0)
                {
                    var ice = iceQueue.Dequeue();
                    await onIceCandidate(Name, PeerConnectionId, ice);
                }
            }
            else
            {
                iceQueue.Enqueue(iceCandidateInit);
            }

        }


    }
}
