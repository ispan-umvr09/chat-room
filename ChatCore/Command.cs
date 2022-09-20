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

    // 將 長度及指令類型 序列化到 Buffer 中
    public virtual void Serialize()
    {
      // 指摽放回開頭，從開頭開始填入資料
      m_Pos = m_BeginPos;

      _WriteToBuffer(m_Length);
      _WriteToBuffer(m_Command);
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
      return m_PacketBuffer;
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
  }
}
