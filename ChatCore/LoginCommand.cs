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

      // write command data
      _WriteToBuffer(m_Name);
    }
  }
}
