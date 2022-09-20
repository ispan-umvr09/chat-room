using System;
using System.Text;

namespace ChatCore
{
  public class Command
  {
    public enum Type
    {
      LOGIN = 1,
      MESSAGE
    }

    private int m_Command;
    public int CommandID => m_Command;

    private int m_Length;

    private byte[] m_PacketBuffer = new byte[1024];
    private int m_BeginPos = 0;
    private int m_Pos = 0;

    protected Command(int command)
    {
      m_Command = command;
    }

    // 抓取 buffer 內的 資料長度 及 指令類型
    public static void FetchHeader(out int length, out int command, byte[] packetData, int beginPos)
    {
      // 建立一個 暫時的 Command Object
      var header = new Command(0);

      // 將 Buffer 填入到 Command Object
      header.UnSealPacketBuffer(packetData, beginPos);
      // 將 Buffer 反序列化，取出資料長度 及 指令類型
      header.Unserialize();

      length = header.m_Length;
      command = header.m_Command;
    }

    // 將 長度及指令類型 序列化到 Buffer 中
    public virtual void Serialize()
    {
      // 指摽放回開頭，從開頭開始填入資料
      m_Pos = m_BeginPos;

      _WriteToBuffer(m_Length);
      _WriteToBuffer(m_Command);
    }

    // 將 Buffer 反序列化，取出長度及指令類型
    public virtual void Unserialize()
    {
      // 回到 Buffer 開頭，從開頭讀取資料
      m_Pos = m_BeginPos;

      // 取出資料長度
      _ReadFromBuffer(out m_Length);
      // 取出指令類型
      _ReadFromBuffer(out m_Command);
    }

    // 抓取已序列化的 Buffer 的內容及長度
    public byte[] SealPacketBuffer(out int iLength)
    {
      m_Length = m_Pos;

      var curPos = m_Pos;
      m_Pos = m_BeginPos;
      _WriteToBuffer(m_Length);
      m_Pos = curPos;

      iLength = m_Length;

      var byteData = new byte[iLength];
      Buffer.BlockCopy(m_PacketBuffer, 0, byteData, 0, byteData.Length);
      return byteData;
    }

    // 將 傳入的buffer 複製到 內部Buffer
    public void UnSealPacketBuffer(byte[] packetData, int beginPos)
    {
      Buffer.BlockCopy(packetData, beginPos, m_PacketBuffer, 0, packetData.Length);
    }

    // 將整數填入 Buffer 中
    protected bool _WriteToBuffer(int i)
    {
      // convert int to byte array
      var bytes = BitConverter.GetBytes(i);
      _WriteToBuffer(bytes);
      return true;
    }

    // 將字串填入 Buffer 中
    protected bool _WriteToBuffer(string str)
    {
      // convert string to byte array
      var bytes = Encoding.Unicode.GetBytes(str);

      // write byte array length to packet's byte array
      if (_WriteToBuffer(bytes.Length) == false)
      {
        return false;
      }

      _WriteToBuffer(bytes);
      return true;
    }

    // 將 byte陣列 填入 Buffer 中
    private void _WriteToBuffer(byte[] byteData)
    {
      // converter little-endian to network's big-endian
      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(byteData);
      }

      byteData.CopyTo(m_PacketBuffer, m_BeginPos + m_Pos);
      m_Pos += byteData.Length;
    }

    // 從 Buffer 讀取 整數
    protected bool _ReadFromBuffer(out int i)
    {
      if (BitConverter.IsLittleEndian)
      {
        var byteData = new byte[sizeof(int)];
        Buffer.BlockCopy(m_PacketBuffer, m_BeginPos + m_Pos, byteData, 0, byteData.Length);
        Array.Reverse(byteData);
        i = BitConverter.ToInt32(byteData, 0);
      }
      else
      {
        i = BitConverter.ToInt32(m_PacketBuffer, m_BeginPos + m_Pos);
      }

      m_Pos += sizeof(int);
      return true;
    }

    // 從 Buffer 讀取 字串
    protected bool _ReadFromBuffer(out string str)
    {
      // read string length
      _ReadFromBuffer(out int length);

      if (BitConverter.IsLittleEndian)
      {
        var byteData = new byte[length];
        Buffer.BlockCopy(m_PacketBuffer, m_BeginPos + m_Pos, byteData, 0, length);
        Array.Reverse(byteData);
        str = Encoding.Unicode.GetString(byteData, 0, length);
      }
      else
      {
        str = Encoding.Unicode.GetString(m_PacketBuffer, m_BeginPos + m_Pos, length);
      }

      m_Pos += length;
      return true;
    }
  }
}
