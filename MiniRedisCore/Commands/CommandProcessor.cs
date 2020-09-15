using System;
using System.Text.RegularExpressions;
using MiniRedisCore.Storage;

namespace MiniRedisCore.Commands
{
  public class CommandProcessor
  {
    public static string Process(MiniRedisStorage storage, string command)
    {
      var parameters = new Regex(@"\s\s+")
        .Replace(command, " ")
        .Split(" ");

      if (parameters.Length == 0)
        throw new ArgumentException("Command is not specified");

      const string nil = "(nil)";
      var cmd = parameters[0];
      return cmd.ToUpperInvariant() switch
      {
        "SET" => (storage.Set(parameters) ?? nil),
        "GET" => (storage.Get(parameters) ?? nil),
        "DEL" => storage.Del(parameters),
        "DBSIZE" => storage.DbSize(parameters),
        "INCR" => storage.Incr(parameters),
        "ZADD" => storage.ZAdd(parameters),
        "ZCARD" => storage.ZCard(parameters),
        "ZRANK" => (storage.ZRank(parameters) ?? nil),
        "ZRANGE" => storage.ZRange(parameters) ?? nil,
        _ => throw new ArgumentException($"Could not recognize command '{cmd}'")
      };
    }
  }
}