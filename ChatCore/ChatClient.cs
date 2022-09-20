using System;
using System.Net.Sockets;

namespace ChatCore
{
  public class ChatClient
  {
    string m_UserName = "";
    private Transmitter m_Transmitter = new Transmitter();
    
    public ChatClient()
    {
    }

    public bool Connect(string address, int port)
    {
      // 使用 Transmitter 連線到 Server 
      if (!m_Transmitter.Connect(address, port))
      {
        Console.WriteLine("Connect to server failed");
        return false;
      }

      // 註冊要關注的指令事件
      m_Transmitter.Register<MessageCommand>(OnMessageCommand);

      // 送出登入指令
      var loginCommand = new LoginCommand
      {
        m_Name = m_UserName
      };
      m_Transmitter.Send(loginCommand);

      return true;
    }

    public void Disconnect()
    {
      m_Transmitter.Disconnect();
      Console.WriteLine("Disconnected");
    }

    public void Refresh()
    {
      // 給予 Transmitter CPU處理時間來檢查是否有收到網路資料要處理
      m_Transmitter.Refresh();
    }

    public void SetName(string name)
    {
      m_UserName = name;
    }

    public void SendMessage(string message)
    {
      var command = new MessageCommand
      {
        m_UserName = m_UserName,
        m_Message = message
      };

      m_Transmitter.Send(command);
    }

    public void OnMessageCommand(Transmitter sender, MessageCommand command)
    {
      Console.WriteLine("{0}: {1}", command.m_UserName, command.m_Message);
    }
  }
}
