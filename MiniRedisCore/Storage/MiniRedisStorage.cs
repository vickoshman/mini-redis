using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MiniRedisCore.Storage
{
  public class MiniRedisStorage
  {
    private readonly ConcurrentDictionary<string, StoredValue> _valueByKey;
    private readonly Timer _removeExpiredTimer;
    private readonly StorageSettings _settings;

    public MiniRedisStorage(StorageSettings settings = null)
    {
      _valueByKey = new ConcurrentDictionary<string, StoredValue>();
      _removeExpiredTimer = new Timer(RemoveExpired, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
      _settings = settings ?? new StorageSettings();
    }

    public int Size => _valueByKey.Count(kvp => !kvp.Value.IsExpired());

    public string Set(string key, string value, int? expirationSeconds)
    {
      var storedValue = new StoredValue(value, StoredValueType.String, expirationSeconds);
      
      _valueByKey.AddOrUpdate(key,
        v => storedValue,
        (k, v) => storedValue);

      return value;
    }

    public string Get(string key)
    {
      if (!_valueByKey.TryGetValue(key, out var stored))
        return null;
      
      AssertStoredType(stored, StoredValueType.String);

      return stored.IsExpired() ? null : stored.Value.ToString();
    }

    public string Delete(string key)
    {
      return _valueByKey.TryRemove(key, out _) ? "1" : "0";
    }

    public string Increment(string key)
    {
      return _valueByKey.AddOrUpdate(key,
        k => StoredValue.NewString("1"), 
        (k, v) =>
        {
          if (v.IsExpired())
          {
            _valueByKey.Remove(k, out _);
            return StoredValue.NewString("1");
          }

          if (!long.TryParse(v.Value.ToString(), out var lValue))
            throw new InvalidOperationException("Operation is limited to 64 bit signed integers");

          var newValue = lValue + 1;
          return StoredValue.NewString(newValue.ToString());
        })
        .Value.ToString();
    }

    public int Add(string key, double score, string value)
    {
      _valueByKey.AddOrUpdate(key,
        k => StoredValue.NewSortedSet(score, value),
        (k, v) =>
        {
          AssertStoredType(v, StoredValueType.SortedSet);
          
          lock (v)
          {
            // Value could have been changed at this point
            AssertStoredType(v, StoredValueType.SortedSet);
            v.AsSortedList().Add(score, value);
            return v;
          }
        });

      return 1;
    }

    public int GetNumberOfElements(string key)
    {
      if (!_valueByKey.TryGetValue(key, out var stored))
        return 0;
      
      AssertStoredType(stored, StoredValueType.SortedSet);
      return stored.AsSortedList().Count;
    }

    public int? GetRank(string key, string value)
    {
      if (!_valueByKey.TryGetValue(key, out var set))
        return null;

      AssertStoredType(set, StoredValueType.SortedSet);

      var index = set.AsSortedList().IndexOfValue(value);
      if (index >= 0)
        return index;

      return null;
    }

    public List<string> GetRange(string key, int from, int to)
    {
      if (!_valueByKey.TryGetValue(key, out var value))
        return null;

      AssertStoredType(value, StoredValueType.SortedSet);
      lock (value)
      {
        // Value could have been changed at this point
        AssertStoredType(value, StoredValueType.SortedSet);

        var set = value.AsSortedList();
        to = to > 0 ? to : set.Values.Count + to;
        return set.Values
          .Skip(from)
          .Take(to - from + 1)
          .ToList();
      }
    }

    private static void AssertStoredType(StoredValue value, StoredValueType required)
    {
      if (value.Type != required)
        throw new InvalidOperationException($"This operation is supported with {required} types only");
    }

    private void RemoveExpired(object state)
    {
      try
      {
        foreach (var (key, storedValue) in _valueByKey)
        {
          if (storedValue.IsExpired())
            _valueByKey.TryRemove(key, out _);
        }
      }
      finally
      {
        _removeExpiredTimer.Change(TimeSpan.FromSeconds(_settings.RemoveExpiredTimeoutInSeconds), TimeSpan.FromMilliseconds(-1));
      }
    }
  }
}