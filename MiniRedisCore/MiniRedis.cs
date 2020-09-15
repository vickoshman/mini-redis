using System;
using MiniRedisCore.Commands;
using MiniRedisCore.Communication;
using MiniRedisCore.Storage;

namespace MiniRedisCore
{
  public class MiniRedis
  {
    private readonly MiniRedisStorage _storage;

    public MiniRedis()
    {
      _storage = new MiniRedisStorage();
    }

    public MiniRedisResponse ProcessCommand(string command)
    {
      try
      {
        var result = CommandProcessor.Process(_storage, command);
        return new MiniRedisResponse(result);
      }
      catch (ArgumentException ex)
      {
        return new MiniRedisResponse(new MiniRedisError(MiniRedisErrorCode.InvalidArgument, $"{ex.Message}"));
      }
      catch (InvalidOperationException ex)
      {
        return new MiniRedisResponse(new MiniRedisError(MiniRedisErrorCode.InvalidOperation, $"{ex.Message}"));
      }
    }
  }
}