using System;
using System.Collections.Generic;

namespace MiniRedisCore.Storage
{
  public class StoredValue
  {
    public readonly object Value;
    public readonly StoredValueType Type;
    private readonly DateTime? _expiration;

    public StoredValue(object value, StoredValueType type)
    {
      Value = value;
      Type = type;
    }
    
    public StoredValue(object value, StoredValueType type, int? expirationSeconds)
    {
      Value = value;
      Type = type;
      
      if (expirationSeconds.HasValue)
        _expiration = DateTime.UtcNow.AddSeconds(expirationSeconds.Value);
    }

    public bool IsExpired()
    {
      return DateTime.UtcNow >= _expiration;
    }
    
    public SortedList<double, string> AsSortedList()
    {
      return (SortedList<double, string>) Value;
    }

    public static StoredValue NewSortedSet(double score, string value)
    {
      return new StoredValue(new SortedList<double, string>(new SortedSetComparer()) {{score, value}}, StoredValueType.SortedSet);
    }

    public static StoredValue NewString(string value)
    {
      return new StoredValue(value, StoredValueType.String);
    }
  }
}