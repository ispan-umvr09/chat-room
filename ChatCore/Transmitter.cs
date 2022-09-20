using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChatCore
{
  public class Transmitter
  {
    private string m_ClientID;
    public string ClientID => m_ClientID;

    private TcpClient m_Client;
    private string m_Address;
    private int m_Port;

    // 指令類型
    private readonly Dictionary<int, Type> m_CommandTypes = new Dictionary<int, Type>();
    // 指令分派
    private readonly Dictionary<int, Delegate> m_CommandActions = new Dictionary<int, Delegate>();

    public Transmitter()
    {
    }

    // 給 Server 端使用的建構子，用來紀錄 Client Agent
    public Transmitter(string clientID, TcpClient client = null)
    {
      m_ClientID = clientID;
      m_Client = client;
    }

    // 與 Server 連線
    public bool Connect(string address, int port)
    {
      m_Address = address;
      m_Port = port;

      m_Client = new TcpClient();

      try
      {
        Console.WriteLine("Connecting to server {0}:{1}", m_Address, m_Port);
        m_Client.Connect(m_Address, m_Port);
        Console.WriteLine("Connected to server");

        return true;
      }
      catch (ArgumentNullException e)
      {
        Console.WriteLine("ArgumentNullException happened: {0}", e);
        return false;
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException happened: {0}", e);
        return false;
      }
      catch (Exception e)
      {
        Console.WriteLine("Exception happened: {0}", e);
        return false;
      }
    }

    // 是否連線中
    public bool IsConnected()
    {
      if (m_Client == null || m_Client.Connected == false)
      {
        return false;
      }

      try
      {
        if (m_Client.Client.Poll(0, SelectMode.SelectRead))
        {
          var buff = new byte[1];
          if (m_Client.Client.Receive(buff, SocketFlags.Peek) == 0)
          {
            return false;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    // 切斷連線
    public void Disconnect()
    {
      m_Client.Close();
    }

    // 註冊要關注的指令
    public void Register<T>(Action<Transmitter, T> action) where T : Command, new()
    {
      var cmd = new T();
      m_CommandTypes.Add(cmd.CommandID, cmd.GetType());
      m_CommandActions.Add(cmd.CommandID, action);
    }

    // 送出指令
    public void Send(Command command)
    {
      command.Serialize();
      var buffer = command.SealPacketBuffer(out var length);

      try
      {
        m_Client.GetStream().Write(buffer, 0, length);
      }
      catch (Exception e)
      {
        Console.WriteLine("Client {0} Send Failed: {1}", ClientID, e.Message);
      }
    }

    // 檢查是否有收到網路資料要處理
    public void Refresh()
    {
      if (m_Client.Available > 0)
      {
        HandleReceiveMessages();
      }
    }

    // 處理網路資料，並且分派指令
    private void HandleReceiveMessages()
    {
      var numBytes = m_Client.Available;
      var buffer = new byte[numBytes];

      var bytesRead = m_Client.GetStream().Read(buffer, 0, numBytes);

      if (bytesRead != numBytes)
      {
        Console.WriteLine("Error reading stream buffer...");
        return;
      }

      var pos = 0;

      while (pos < bytesRead)
      {
        Command.FetchHeader(out var length, out var commandId, buffer, pos);

        var t = m_CommandTypes[commandId];
        var msg = (Command)Activator.CreateInstance(t); // 根據類型 動態建立物件
        msg.UnSealPacketBuffer(buffer, pos);
        msg.Unserialize();

        // 找出關注此指令的對象，並且傳送
        var actionObj = m_CommandActions[commandId];
        actionObj.DynamicInvoke(new object[] { this, msg });

        pos += length;
      }
    }
  }
}
