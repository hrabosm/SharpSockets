using System;
using System.Collections.Generic;
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
            this.sharpSocket.Connect(this.endIP);
            this.receiveT.Start();
        }
        public void Send(string stringData)
        {
            System.Threading.Thread.Sleep(100);
            this.dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            this.sharpSocket.Send(this.dataTs);
        }
        private void Listen()
        {
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
            if(OnDataReceived != null)
                OnDataReceived(this, e);
        }
        public void Stop()
        {
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
        private List<EndPoint> clients = new List<EndPoint>();
        public event OnDataReceivedEventHandler OnDataReceived;
        public Socket sharpSocket;
        private IPEndPoint endIP;
        //private Thread receiveT;
        private byte[] data = new byte[2048];
        public string dataS { get; private set; }
        private int id = 0;
        public Server(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            this.sharpSocket = new Socket(addressFamily,socketType,protocolType);
            //this.receiveT = new Thread(this.Listen);
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
            while(true)
            {
                this.sharpSocket.Bind(this.endIP);
                this.sharpSocket.Listen(backlog);
                this.sharpSocket.Accept();
                Thread t = new Thread(Listen);
                t.Start();
                clients[this.id] = this.sharpSocket.RemoteEndPoint;
                this.id++;
            }
        }
        private void Listen()
        {
            while(!this.sharpSocket.Connected){}
            
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
            if(OnDataReceived != null)
                OnDataReceived(this, e);
        }
        /*
        public void Stop()
        {
            if(this.receiveT.IsAlive)
            {
                this.receiveT.Interrupt();
            }
            this.sharpSocket.Shutdown(SocketShutdown.Receive);
            this.sharpSocket.Close();
        }
        */
        public void Send(string stringData,int id)
        {
            System.Threading.Thread.Sleep(100);
            byte[] dataTs = System.Text.Encoding.ASCII.GetBytes(stringData);
            try
            {
                this.sharpSocket.SendTo(dataTs,clients[id]);
            }
            catch
            {
                Console.WriteLine("User {0} is not connected!",id);
            }
        }
    }
}