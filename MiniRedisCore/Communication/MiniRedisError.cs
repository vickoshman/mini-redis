namespace MiniRedisCore.Communication
{
  public class MiniRedisError
  {
    public MiniRedisErrorCode Code;
    public string Message;

    public MiniRedisError(MiniRedisErrorCode code, string message)
    {
      Code = code;
      Message = message;
    }
    
    public override string ToString()
    {
      return $"ERROR {Code}: {Message}";
    }
  }
}