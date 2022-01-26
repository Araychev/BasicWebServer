using System.Net;
using System.Net.Sockets;
using System.Text;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;

namespace BasicWebServer.Server
{
    public class HttpServer
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpListener _serverListener;

        private readonly RoutingTable _routingTable;


        public HttpServer(int port, Action<IRoutingTable> routingTable)
            : this("127.0.0.1", port, routingTable)
        {

        }

        public HttpServer(Action<IRoutingTable> routingTable)
            : this(8080, routingTable)
        {

        }
        public HttpServer(string ipAddress,
            int port,
            Action<IRoutingTable> routingTableConfiguration)
        {
            IPAddress ipAddress1;
            this._ipAddress = IPAddress.Parse(ipAddress);
            this._port = port;

            this._serverListener = new TcpListener(this._ipAddress, port);
            routingTableConfiguration(this._routingTable = new RoutingTable());
        }


        public async Task Start()
        {
            this._serverListener.Start();

            Console.WriteLine($"Server started on port {_port}...");
            Console.WriteLine("Listening fo request...");

            while (true)
            {
                var connection = await _serverListener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    var networkStream = connection.GetStream();
                    var requestText = await this.ReadRequest(networkStream);
                    Console.WriteLine(requestText);
                    var request = Request.Parse(requestText);
                    var response = this._routingTable.MatchRequest(request);

                    //Execute pre-render action for the response
                    if (response.PreRenderAction != null)
                    {
                        response.PreRenderAction(request, response);
                    }

                    await WriteResponse(networkStream, response);

                    connection.Close();

                });
            }
        }

        private async Task WriteResponse(NetworkStream networkStream, Response message)
        {
            var contentLenght = Encoding.UTF8.GetByteCount(message.ToString());

            var response = $@"HTTP/1.1 200 OK " + " " +
                           $@"Content-Type: text/plain; charset=UTF-8" + " " +
                           $@"Content-Length: {contentLenght}" + " " +
                           $@"{message}";
            var resposeBytes = Encoding.UTF8.GetBytes(response.ToString());
            await networkStream.WriteAsync(resposeBytes);
        }

        private async Task<string> ReadRequest(NetworkStream networkStream)
        {
            var bufferLength = 1024;
            var buffer = new byte[bufferLength];
            var totalBytes = 0;


            var requestBuilder = new StringBuilder();

            do
            {
                var byteRead = await networkStream.ReadAsync(buffer, 0, bufferLength);
                totalBytes += byteRead;
                if (totalBytes > 10 * 1024)
                {
                    throw new InvalidOperationException("Request is too large.");
                }

                requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, byteRead));
            }
            //May not run correctly over internet
            while (networkStream.DataAvailable);

            return requestBuilder.ToString().TrimEnd();
        }
    }
}