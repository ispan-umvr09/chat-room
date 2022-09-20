using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatCore
{
  public class ChatServer
  {
    private int m_port;
    private TcpListener m_listener;
    private Thread m_handleThread;

    private readonly Dictionary<string, Transmitter> m_transmitters = new Dictionary<string, Transmitter>();
    private readonly Dictionary<string, string> m_userNames = new Dictionary<string, string>();

    public ChatServer()
    {
    }

    public void Bind(int port)
    {
      m_port = port;

      m_listener = new TcpListener(IPAddress.Any, port);
      Console.WriteLine("Server start at port {0}", port);
      m_listener.Start();
    }

    public void Start()
    {
      m_handleThread = new Thread(ClientsHandler);
      m_handleThread.Start();

      while (true)
      {
        Console.WriteLine("Waiting for a connection... ");
        var client = m_listener.AcceptTcpClient();

        var clientId = client.Client.RemoteEndPoint.ToString();
        Console.WriteLine("Client has connected from {0}", clientId);

        // 建立 Transmitter，並且註冊要接收的指令
        var transmitter = new Transmitter(clientId, client);
        transmitter.Register<LoginCommand>(OnLoginCommand);
        transmitter.Register<MessageCommand>(OnMessageCommand);

        // 將 transmitter 加入到列表中
        lock (m_transmitters)
        {
          m_transmitters.Add(clientId, transmitter);
          m_userNames.Add(clientId, "Unknown");
        }
      }
    }

    private void ClientsHandler()
    {
      while (true)
      {
        // 複製 transmitters，減少 Lock(佔用) `m_transmitters` 記憶體的時間
        Dictionary<string, Transmitter> transmitters;
        lock (m_transmitters)
        {
          transmitters = new Dictionary<string, Transmitter>(m_transmitters);
        }

        // 對每一個 transmitter 做資料接收的處理
        foreach (var clientId in transmitters.Keys)
        {
          var transmitter = transmitters[clientId];
          var isDisconnected = false;

          if (transmitter.IsConnected() == false)
          {
            isDisconnected = true;
          }
          else
          {
            try
            {
              // 檢查網路資料，並且分派指令
              transmitter.Refresh();
            }
            catch (Exception e)
            {
              Console.WriteLine("Client {0} Refresh Error: {1}", clientId, e.Message);
              isDisconnected = true;
            }
          }

          // 處理斷線
          if (isDisconnected)
          {
            lock (m_transmitters)
            {
              if (m_transmitters.Keys.Contains(clientId))
              {
                Console.WriteLine("Client {0} has disconnected...", clientId);
                m_transmitters.Remove(clientId);
                m_userNames.Remove(clientId);
                transmitter.Disconnect();
              }
            }
          }
        }
      }
    }

    // 處理 登入 指令事件
    public void OnLoginCommand(Transmitter transmitter, LoginCommand cmd)
    {
      m_userNames[transmitter.ClientID] = cmd.m_Name;
      Console.WriteLine("Client {0} Login from {1}",
        m_userNames[transmitter.ClientID], transmitter.ClientID);
    }

    // 處理 聊天訊息 指令事件
    public void OnMessageCommand(Transmitter sender, MessageCommand cmd)
    {
      Console.WriteLine(cmd.m_UserName + " say: " + cmd.m_Message);

      Broadcast(sender, cmd.m_Message);
    }

    // 廣播聊天訊息
    private void Broadcast(Transmitter sender, string message)
    {
      var command = new MessageCommand
      {
        m_UserName = m_userNames[sender.ClientID],
        m_Message = message
      };

      // 複製 transmitters，減少 Lock(佔用) `m_transmitters` 記憶體的時間
      Dictionary<string, Transmitter> transmitters;
      lock (m_transmitters)
      {
        transmitters = new Dictionary<string, Transmitter>(m_transmitters);
      }

      // 傳送訊息給其他玩家
      foreach (var transmitter in transmitters.Values)
      {
        if (transmitter != sender)
        {
          transmitter.Send(command);
        }
      }
    }
  }
}
