using System;
using ChatCore;

namespace ChatCoreTest
{
  internal class Program
  {
    private static byte[] m_PacketData;
    private static uint m_Pos;

    public static void Main(string[] args)
    {
      var loginCommand = new LoginCommand { m_Name = "Arthur" };
      var messageCommand = new MessageCommand { m_UserName = "JoJo", m_Message = "Hello!" };

      // 傳送端：將資料序列化
      var length1 = SerializeCommand(loginCommand, out var buffer1);
      printBuffer(buffer1, length1);
      
      var length2 = SerializeCommand(messageCommand, out var buffer2);
      printBuffer(buffer2, length2);
    }

    // 將 Command 序列化，並且抓取 Buffer內容 及 長度
    private static int SerializeCommand(Command command, out byte[] buffer)
    {
      command.Serialize();
      buffer = command.SealPacketBuffer(out var length);

      return length;
    }
    
    // 顯示 Buffer 內容
    private static void printBuffer(byte[] buffer, int length)
    {
      Console.Write($"Output Byte array(length:{length}): ");
      for (var i = 0; i < length; i++)
      {
        Console.Write(buffer[i] + ", ");
      }

      Console.WriteLine("");
    }
  }
}
