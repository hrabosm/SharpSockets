using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace SharpSockets
{
    public delegate void OnDataReceivedEventHandler(object sender, EventArgs e);
    public class Client
    {
        public event OnDataReceivedEventHandler OnDataReceived;
        public Socket sharpSocket;
        private IPEndPoint endIP;
        private Thread receiveT;
        private byte[] data = new byte[2048];
        public string dataS { get; private set; }
        private byte[] dataTs;
        public Client(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            //Construction of Client object
            this.sharpSocket = new Socket(addressFamily,socketType,protocolType);
            this.receiveT = new Thread(Listen);
        }
        public void Start(IPEndPoint endPoint)
        {
            //Connect to endPoint type
            this.endIP = endPoint;
            Connect();
        }
        public void Start(IPAddress ip, int port)
        {
            //Connect to IP type IPAddress and Port
            this.endIP = new IPEndPoint(ip,port);
            Connect();
        }
        public void Start(string ip, int port)
        {
            //Connect to IP type string and Port
            this.endIP = new IPEndPoint(IPAddress.Parse(ip),port);
            Connect();
        }
        private void Connect()
        {
            //Connection process
            this.sharpSocket.Connect(this.endIP);
            this.receiveT.Start();
        }
        public void Send(string stringData)
        {
            //Sending data in form of string
            System.Threading.Thread.Sleep(100);
            this.dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            this.sharpSocket.Send(this.dataTs);
        }
        private void Listen()
        {
            //Listening thread
            while((true))
            {
                if(this.sharpSocket.Connected)
                {
                    try
                    {
                        this.dataS = null;
                        int bytesRecC = this.sharpSocket.Receive(this.data);
                        if(bytesRecC>0)
                        {
                            this.dataS = Encoding.ASCII.GetString(this.data,0, bytesRecC);
                            OnReceived(EventArgs.Empty);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Server ukončil spojení!");
                        break;
                    }
                }
            }
        }
        protected virtual void OnReceived(EventArgs e)
        {
            //event OnDataReceived
            if(OnDataReceived != null)
                OnDataReceived(this, e);
        }
        public void Stop()
        {
            //Stops listening thread if active
            if(this.receiveT.IsAlive)
            {
                this.receiveT.Interrupt();
            }
            this.sharpSocket.Shutdown(SocketShutdown.Receive);
            this.sharpSocket.Close();
        }
    }
    public class Server
    {
        public event OnDataReceivedEventHandler OnDataReceived;
        public Socket sharpSocket;
        private IPEndPoint endIP;
        private Thread receiveT;
        private byte[] data = new byte[2048];
        public string dataS { get; private set; }
        private byte[] dataTs;
        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            //Constructor of Server object
            this.sharpSocket = new Socket(addressFamily,socketType,protocolType);
            this.receiveT = new Thread(this.Listen);
        }
        public void Start(IPEndPoint endPoint, int backlog = 1)
        {
            //Bind to endPoint type with optional backlog
            this.endIP = endPoint;
            OpenListener(backlog);
        }
        public void Start(IPAddress localIp, int port, int backlog = 1)
        {
            //Bind to IP IPAdress type and port with optional backlog
            this.endIP = new IPEndPoint(localIp,port);
            OpenListener(backlog);
        }
        public void Start(string localIp, int port, int backlog = 1)
        {
            //Bind to IP string type and port with optional backlog
            this.endIP = new IPEndPoint(IPAddress.Parse(localIp),port);
            OpenListener(backlog);
        }
        private void OpenListener(int backlog)
        {
            //Bind to endIP and starts listening thread
            this.sharpSocket.Bind(this.endIP);
            this.sharpSocket.Listen(backlog);
            this.receiveT.Start();
        }
        private void Listen()
        {
            //Listening thread
            this.sharpSocket = this.sharpSocket.Accept();
            while((true))
            {
                if(this.sharpSocket.Connected)
                {
                    try
                    {
                        this.dataS = null;
                        int bytesRecS = this.sharpSocket.Receive(this.data);
                        if(bytesRecS>0)
                        {
                            this.dataS = Encoding.ASCII.GetString(this.data,0, bytesRecS);
                            OnReceived(EventArgs.Empty);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Client ukončil spojení");
                        break;
                    }
                }
            }   
        }
        protected virtual void OnReceived(EventArgs e)
        {
            //event OnDataReceived
            if(OnDataReceived != null)
                OnDataReceived(this, e);
        }
        public void Stop()
        {
            //Stops listening thread if running
            if(this.receiveT.IsAlive)
            {
                this.receiveT.Interrupt();
            }
            this.sharpSocket.Shutdown(SocketShutdown.Receive);
            this.sharpSocket.Close();
        }
        public void Send(string stringData)
        {
            //Sends data to last connected client
            System.Threading.Thread.Sleep(100);
            while(!this.sharpSocket.Connected)
            {

            }
            this.dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            this.sharpSocket.Send(this.dataTs);
        }
    }
}