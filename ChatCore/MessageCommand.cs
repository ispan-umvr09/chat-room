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

      // write command data
      _WriteToBuffer( m_UserName );
      _WriteToBuffer( m_Message );
    }
  }
}
