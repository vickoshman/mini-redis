namespace MiniRedisCore.Communication
{
  public class MiniRedisResponse
  {
    public MiniRedisError Error;
    public string Result;

    public MiniRedisResponse(string result)
    {
      Result = result;
    }

    public MiniRedisResponse(MiniRedisError error)
    {
      Error = error;
    }
  }
}