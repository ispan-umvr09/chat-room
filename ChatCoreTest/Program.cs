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

      // 接收端：將 Buffer 反序列化回 Command Object
      var command1 = UnserializeBuffer(buffer1);
      printCommand(command1);

      var command2 = UnserializeBuffer(buffer2);
      printCommand(command2);
    }

    // 將 Command 序列化，並且抓取 Buffer內容 及 長度
    private static int SerializeCommand(Command command, out byte[] buffer)
    {
      command.Serialize();
      buffer = command.SealPacketBuffer(out var length);

      return length;
    }

    // 將 Buffer 反序列化回 Command Object
    private static Command UnserializeBuffer(byte[] buffer)
    {
      // 抓取 buffer 內的 資料長度 及 指令類型
      Command.FetchHeader(out var length, out var commandType, buffer, 0);
      Console.WriteLine("Command: {0}, Length: {1}", (Command.Type)commandType, length);

      Command command;

      // 根據指令類型產生 Command Object
      switch (commandType)
      {
        case (int)Command.Type.LOGIN:
          command = new LoginCommand();
          break;
        case (int)Command.Type.MESSAGE:
          command = new MessageCommand();
          break;
        default:
          // invalid command type
          return null;
      }

      // 將 Buffer 填入到 Command Object
      command.UnSealPacketBuffer(buffer, 0);
      // 將 Buffer 反序列化，取出資料內容
      command.Unserialize();

      return command;
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

    // 顯示 Command 內容
    private static void printCommand(Command command)
    {
      if (command == null)
      {
        return;
      }

      switch (command.CommandID)
      {
        case (int)Command.Type.LOGIN:
          Console.WriteLine("Login Name: {0}", ((LoginCommand)command).m_Name);
          break;
        case (int)Command.Type.MESSAGE:
          Console.WriteLine("{0} say: {1}", ((MessageCommand)command).m_UserName, ((MessageCommand)command).m_Message);
          break;
      }
    }
  }
}
