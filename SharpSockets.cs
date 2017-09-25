using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace SharpSockets
{
    public class Client
    {
        public Socket sharpSocket;
        private IPEndPoint endIP;
        private Thread receiveT;
        private byte[] data = new byte[2048];
        public string dataS;
        private byte[] dataTs;
        public Client(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            this.sharpSocket = new Socket(addressFamily,socketType,protocolType);
            this.receiveT = new Thread(Listen);
        }
        public void Start(IPEndPoint endPoint)
        {
            this.endIP = endPoint;
            Connect();
        }
        public void Start(IPAddress ip, int port)
        {
            this.endIP = new IPEndPoint(ip,port);
            Connect();
        }
        public void Start(string ip, int port)
        {
            this.endIP = new IPEndPoint(IPAddress.Parse(ip),port);
            Connect();
        }
        private void Connect()
        {
            int i = 1;
            start:
            try
            {
                this.sharpSocket.Connect(this.endIP);
                this.receiveT.Start();
            }
            catch (Exception)
            {
                if( i <= 5 )
                {
                    Console.WriteLine("Connecting");
                    System.Threading.Thread.Sleep(1000);
                    i++;
                    goto start;
                }
                else
                {
                    Console.WriteLine("Connection failed!");
                }
            }
        }
        public void Send(string stringData)
        {
            System.Threading.Thread.Sleep(100);
            this.dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            this.sharpSocket.Send(this.dataTs);
        }
        private void Listen()
        {
            if(this.sharpSocket.Connected)
            {
                while((true))
                {
                    this.dataS = null;
                    int bytesRecC = this.sharpSocket.Receive(this.data);
                    if(bytesRecC>1)
                    {
                        this.dataS = Encoding.ASCII.GetString(this.data,0, bytesRecC);
                        Console.WriteLine(this.dataS+"D");
                    }
                }
            }
            else
            {
                Console.WriteLine("Wut");
            }   
        }
    }
    public class Server
    {
        public Socket sharpSocket;
        private IPEndPoint endIP;
        private Thread receiveT;
        private byte[] data = new byte[2048];
        public string dataS;
        private byte[] dataTs;
        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            this.sharpSocket = new Socket(addressFamily,socketType,protocolType);
            this.receiveT = new Thread(this.Listen);
        }
        public void Start(IPEndPoint endPoint, int backlog = 1)
        {
            this.endIP = endPoint;
            OpenListener(backlog);
        }
        public void Start(IPAddress localIp, int port, int backlog = 1)
        {
            this.endIP = new IPEndPoint(localIp,port);
            OpenListener(backlog);
        }
        public void Start(string localIp, int port, int backlog = 1)
        {
            this.endIP = new IPEndPoint(IPAddress.Parse(localIp),port);
            OpenListener(backlog);
        }
        private void OpenListener(int backlog)
        {
            this.sharpSocket.Bind(this.endIP);
            this.sharpSocket.Listen(backlog);
            this.receiveT.Start();
        }
        private void Listen()
        {
            this.sharpSocket = this.sharpSocket.Accept();
            if(this.sharpSocket.Connected)
            {
                while((true))
                {
                    this.dataS = null;
                    int bytesRecS = this.sharpSocket.Receive(this.data);
                    if(bytesRecS>1)
                    {
                        this.dataS = Encoding.ASCII.GetString(this.data,0, bytesRecS);
                        Console.WriteLine(this.dataS);
                    }
                }
            }   
        }
        public void Stop()
        {
            if(this.receiveT.IsAlive)
            {
                this.receiveT.Abort();
            }
            this.sharpSocket.Shutdown(SocketShutdown.Receive);
            this.sharpSocket.Close();
        }
        public void Send(string stringData)
        {
            System.Threading.Thread.Sleep(100);
            while(!this.sharpSocket.Connected)
            {

            }
            this.dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            this.sharpSocket.Send(this.dataTs);
        }
    }
}