namespace ChatCore
{
  public class MessageCommand : Command
  {
    public string m_UserName = "";
    public string m_Message = "";

    public MessageCommand() : base( (int)Type.MESSAGE )
    {
    }

    // 將全部資料序列化到 Buffer 中
    public override void Serialize()
    {
      // 將 長度及指令類型 序列化到 Buffer 中
      base.Serialize();

      // 序列化資料
      _WriteToBuffer( m_UserName );
      _WriteToBuffer( m_Message );
    }

    // 將 Buffer 反序列化，取出資料內容
    public override void Unserialize()
    {
      // 取出長度及指令類型 
      base.Unserialize();

      // 取出資料
      _ReadFromBuffer( out m_UserName );
      _ReadFromBuffer( out m_Message );
    }
  }
}
