using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Newtonsoft.Json;

namespace PrsiGame.WebSockets
{
    public sealed class JsonWebSocket : IDisposable
    {
        private readonly string _name;
        private readonly Action<string> _log;
        private const int BufferSize = 1024 * 4;

        public JsonWebSocket(string name, WebSocket webSocket, Action<string> log = null)
        {
            _name = name;
            _log = log;
            Id = Guid.NewGuid();
            WebSocket = webSocket;
        }

        public Guid Id { get; private set; }

        public WebSocketState State => WebSocket.State;

        public WebSocket WebSocket { get; private set; }

        public async Task SendAsync<T>(T dto, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(dto, Formatting.None);
            var utf8 = Encoding.UTF8.GetBytes(json);
            var l = $"{_name}: Sending {Encoding.UTF8.GetString(utf8)}";
            Console.WriteLine(l);
            if (_log != null)
            {
                _log(l);
            }

            try
            {
                await WebSocket.SendAsync(new ArraySegment<byte>(utf8), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                if (_log != null)
                {
                    _log(e.ToString());
                }

                Dispose();
            }

            Console.WriteLine($"{_name}: Sending done.");
            if (_log != null)
            {
                _log($"{_name}: Sending done.");
            }
        }

        public async Task<Result<T>> ReceiveAsync<T>(CancellationToken cancellationToken) where T : class
        {
            var receiveResult = await ReceiveInternalAsync(cancellationToken);

            return receiveResult.Bind(bytes =>
            {
                try
                {
                    var socketMessage = Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
                    return Result.Ok(JsonConvert.DeserializeObject<T>(socketMessage));
                }
                catch (Exception e)
                {
                    return new ParsingCommandError();
                }
            });
        }

        public async Task<Result<IDictionary<string, object>>> ReceiveJsonAsync(CancellationToken cancellationToken)
        {
            var receiveResult = await ReceiveInternalAsync(cancellationToken);

            return receiveResult.Bind(bytes =>
            {
                try
                {
                    var socketMessage = Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
                    return Result.Ok(JsonConvert.DeserializeObject<IDictionary<string, object>>(socketMessage));
                }
                catch (Exception e)
                {
                    return new ParsingCommandError();
                }
            });
        }

        // TODO: this definitely does not belong here.
        public T Convert<T>(object doc) where T : class
        {
            var json = JsonConvert.SerializeObject(doc);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<Result<List<byte>>> ReceiveInternalAsync(CancellationToken cancellationToken)
        {
            var l = $"{_name} receiving.";
            Console.WriteLine(l);
            if (_log != null)
            {
                _log(l);
            }

            if (WebSocket.CloseStatus != null)
            {
                return new WebSocketClosedError();
            }

            var buffer = new byte[BufferSize];
            WebSocketReceiveResult result;
            try
            {
                result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            catch (IOException ex)
            {
                // await WebSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Close", cancellationToken);
                Dispose();
                return new WebSocketUnrecoverableError();
            }

            if (result.CloseStatus != null)
            {
                await WebSocket.CloseOutputAsync(result.CloseStatus.Value, "Close", cancellationToken);
                Dispose();
                return new WebSocketClosedError();
            }

            var bytes = new List<byte>();

            while (!result.EndOfMessage)
            {
                bytes.AddRange(buffer.Take(result.Count));
                result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }

            bytes.AddRange(buffer.Take(result.Count));
            var l2 = $"{_name} received {Encoding.UTF8.GetString(bytes.ToArray())}.";
            Console.WriteLine(l);
            if (_log != null)
            {
                _log(l2);
            }

            return bytes;
        }

        public void Dispose()
        {
            Console.WriteLine($"{_name} Disposed.");
            WebSocket.Dispose();
        }
    }
}
