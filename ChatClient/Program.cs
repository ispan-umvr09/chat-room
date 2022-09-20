using System;
using System.Net.Sockets;

namespace ChatClient
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("====================================");
      var client = new ChatCore.ChatClient();

      Console.WriteLine( "<Please enter your name...>" );
      var name = Console.ReadLine();
      client.SetName( name );

      var succeed = client.Connect("127.0.0.1", 4099);

      if (!succeed)
      {
        return;
      }

      Console.WriteLine("<You can press any key to start entering text...>");

      while (true)
      {
        while( Console.KeyAvailable == false )
        {
          client.Refresh();
          System.Threading.Thread.Sleep( 1 );
        }

        var msg = Console.ReadLine();

        if (msg == "exit")
        {
          Console.WriteLine("<Bye...>");
          client.Disconnect();
          break;
        }

        client.SendMessage(msg);
        Console.WriteLine("<Message sent, press any key to start entering text again...>");
      }
    }
  }
}
