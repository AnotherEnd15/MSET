using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityWebSocket;
using ErrorEventArgs = UnityWebSocket.ErrorEventArgs;

namespace ET
{
    public class WChannel: AChannel
    {
        public WebSocket WebSocketContext { get; private set; }
        
        private Queue<byte[]> sendbuffer = new ();

        private bool isSending;

        private bool isConnected;

        private int recvCount;

        private string address;

        private Action<int> OnError;

        public WChannel(long id, string address,Action<int> onError)
        {
            Log.Debug($"创建Websokcet: {id} {address}");
            this.Id = id;
            this.ChannelType = ChannelType.Accept;
            this.address = address;
            this.OnError = onError;
            this.WebSocketContext = new WebSocket(address);

            this.WebSocketContext.ConnectAsync();

            this.WebSocketContext.OnOpen += WebSocketContext_OnOpen;
            this.WebSocketContext.OnClose += WebSocketContext_OnClose;
            this.WebSocketContext.OnError += WebSocketContext_OnError;
            this.WebSocketContext.OnMessage += WebSocketContext_OnMessage;
            
        }

        public void SetBuffer(Queue<byte[]> cache)
        {
            sendbuffer = cache;
        }



        private void WebSocketContext_OnMessage(object sender, MessageEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.OnRecv(e.RawData);
        }

        private void WebSocketContext_OnError(object sender, ErrorEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            Log.Error($"和服务器通信出现错误 {this.address} {e.Exception?.ToString()} {e.Message}");
            this.isConnected = false;
        }

        private void WebSocketContext_OnClose(object sender, CloseEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.isConnected = false;
            Log.Error($"WSocket链接关闭 {e.StatusCode} {e.Reason}");
            this.OnError?.Invoke(ErrorCore.ERR_WebsocketPeerReset);
        }

        private void WebSocketContext_OnOpen(object sender, OpenEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.isConnected = true;
            Log.Debug($"链接服务器成功 SendBuffer: {this.sendbuffer.Count}");
            this.StartSend();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.Id = 0;
            this.WebSocketContext.CloseAsync();
        }

        public void Send(byte[] stream)
        {
            this.sendbuffer.Enqueue(stream);

            if (this.isConnected)
            {
                this.StartSend();
            }
        }

        public void StartSend()
        {
            if (this.IsDisposed)
            {
                return;
            }
            try
            {
                if (this.isSending)
                {
                    return;
                }
                this.isSending = true;

                while (true)
                {
                    if (this.sendbuffer.Count == 0)
                    {
                        this.isSending = false;
                        return;
                    }
                    var bytes = this.sendbuffer.Dequeue();
                    try
                    {
                        this.WebSocketContext.SendAsync(bytes);
                        
                        if (this.IsDisposed)
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        this.sendbuffer.Enqueue(bytes);
                        Log.Error(e);
                        this.OnError(ErrorCore.ERR_WebsocketSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        private void OnRecv(byte[] rawData)
        {
            ushort opcode = BitConverter.ToUInt16(rawData, Packet.KcpOpcodeIndex);
            Type type = NetServices.Instance.GetType(opcode);
            try
            {
                var message = SerializeHelper.Deserialize(type, rawData, Packet.OpcodeLength, rawData.Length - Packet.OpcodeLength);
                NetServices.Instance.OnRead(this.Id, message);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}