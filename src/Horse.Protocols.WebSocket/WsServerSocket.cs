using System;
using System.IO;
using System.Threading.Tasks;
using Horse.Core;

namespace Horse.Protocols.WebSocket
{
    /// <summary>
    /// Websocket Server socket object
    /// </summary>
    public class WsServerSocket : SocketBase, IHorseWebSocket
    {
        /// <summary>
        /// WebSocketWriter singleton instance
        /// </summary>
        private static readonly WebSocketWriter _writer = new WebSocketWriter();

        /// <summary>
        /// Server of the socket
        /// </summary>
        public IHorseServer Server { get; }

        /// <summary>
        /// Socket's connection information
        /// </summary>
        public IConnectionInfo Info { get; }

        private Action<WsServerSocket> _cleanupAction;

        /// <summary>
        /// Creates new server-side websocket client
        /// </summary>
        public WsServerSocket(IHorseServer server, IConnectionInfo info)
            : base(info)
        {
            Client = info.Client;
            Server = server;
            Info = info;
        }

        /// <summary>
        /// Completed disconnected operations for websocket client
        /// </summary>
        protected override void OnDisconnected()
        {
            if (_cleanupAction != null)
                _cleanupAction(this);

            base.OnDisconnected();
        }

        /// <summary>
        /// Runs cleanup action
        /// </summary>
        internal void SetCleanupAction(Action<WsServerSocket> action)
        {
            _cleanupAction = action;
        }

        /// <summary>
        /// Sends websocket ping message
        /// </summary>
        public override void Ping()
        {
            Send(PredefinedMessages.PING);
        }

        /// <summary>
        /// Sends websocket pong message
        /// </summary>
        public override void Pong(object pingMessage = null)
        {
            if (pingMessage == null)
            {
                Send(PredefinedMessages.PONG);
                return;
            }

            WebSocketMessage ping = pingMessage as WebSocketMessage;
            if (ping == null)
            {
                Send(PredefinedMessages.PONG);
                return;
            }

            WebSocketMessage pong = new WebSocketMessage();
            pong.OpCode = SocketOpCode.Pong;
            pong.Masking = ping.Masking;
            if (ping.Length > 0)
                pong.Content = new MemoryStream(ping.Content.ToArray());

            byte[] data = _writer.Create(pong).Result;
            Send(data);
        }

        /// <summary>
        /// Sends websocket message to client
        /// </summary>
        public bool Send(WebSocketMessage message)
        {
            byte[] data = _writer.Create(message).Result;
            return Send(data);
        }

        /// <summary>
        /// Sends websocket message to client
        /// </summary>
        public async Task<bool> SendAsync(WebSocketMessage message)
        {
            byte[] data = await _writer.Create(message);
            return Send(data);
        }

        /// <summary>
        /// Sends string message to client
        /// </summary>
        public bool Send(string message)
        {
            byte[] data = _writer.Create(WebSocketMessage.FromString(message)).Result;
            return Send(data);
        }

        /// <summary>
        /// Sends string message to client
        /// </summary>
        public async Task SendAsync(string message)
        {
            byte[] data = await _writer.Create(WebSocketMessage.FromString(message));
            Send(data);
        }
    }
}