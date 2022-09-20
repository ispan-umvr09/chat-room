namespace ChatCore
{
  public class LoginCommand : Command
  {
    public string m_Name = "";

    public LoginCommand() : base((int)Type.LOGIN)
    {
    }

    // 將全部資料序列化到 Buffer 中
    public override void Serialize()
    {
      // 將 長度及指令類型 序列化到 Buffer 中
      base.Serialize();

      // 序列化資料
      _WriteToBuffer(m_Name);
    }
    
    // 將 Buffer 反序列化，取出資料內容
    public override void Unserialize()
    {
      // 取出長度及指令類型 
      base.Unserialize();

      // 取出資料
      _ReadFromBuffer(out m_Name);
    }
  }
}
