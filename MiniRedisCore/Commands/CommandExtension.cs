using System;
using System.Linq;
using MiniRedisCore.Storage;

namespace MiniRedisCore.Commands
{
  public static class CommandExtensions
  {
    public static string Set(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 3, 4);

      if (parameters.Length == 3)
        return storage.Set(parameters[1], parameters[2], null);
      
      var secondsParam = parameters[3];
      if (!int.TryParse(secondsParam, out var seconds))
        throw new ArgumentException($"Unable to parse {secondsParam}, number of seconds was expected");

      var key = parameters[1];
      var value = parameters[2];
      return storage.Set(key, value, seconds);
    }
    
    public static string Get(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 2);
      
      var key = parameters[1];
      return storage.Get(key);
    }

    public static string Del(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 2);
      
      var key = parameters[1];
      return storage.Delete(key);
    }

    public static string DbSize(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 1);
      return storage
        .Size
        .ToString();
    }

    public static string Incr(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 2);
      
      var key = parameters[1];
      return storage.Increment(key);
    }

    public static string ZAdd(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsAtLeast(parameters, 3);
      
      var key = parameters[1];
      int added = 0;
      for (int i = 2; i < parameters.Length; i += 2)
      {
        if (!double.TryParse(parameters[i], out var score))
          throw new ArgumentException($"Unable to parse score parameter: {score}");
        
        if (i + 1 >= parameters.Length)
          throw new ArgumentException($"Unable to find value for score {score}");
        
        var value = parameters[i + 1];
        
        added += storage.Add(key, score, value);
      }

      return added.ToString();
    }

    public static string ZCard(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 2);
      
      var key = parameters[1];
      return storage.GetNumberOfElements(key).ToString();
    }

    public static string ZRank(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 3);

      var key = parameters[1];
      var value = parameters[2];
      var rank = storage.GetRank(key, value);
      return rank?.ToString();
    }

    public static string ZRange(this MiniRedisStorage storage, string[] parameters)
    {
      AssertLengthIsExactly(parameters, 4);

      var key = parameters[1];
      if (!int.TryParse(parameters[2], out var from))
        throw new ArgumentException($"Unable to parse from parameter: {parameters[2]}");
      
      if (!int.TryParse(parameters[3], out var to))
        throw new ArgumentException($"Unable to parse to parameter: {parameters[3]}");

      var result = storage.GetRange(key, from, to);
      return result == null ? null : string.Join(Environment.NewLine, result);
    }

    private static void AssertLengthIsExactly(string[] parameters, params int[] requiresEither)
    {
      if (!requiresEither.Contains(parameters.Length))
        throw new ArgumentException($"Invalid number of parameters for {parameters[0]} command");
    }
    
    private static void AssertLengthIsAtLeast(string[] parameters, int atLeast)
    {
      if (parameters.Length < atLeast)
        throw new ArgumentException($"Invalid number of parameters for {parameters[0]} command");
    }
  }
}