using System;
using System.Linq;
using System.Threading;
using MiniRedisCore;
using MiniRedisCore.Communication;
using NUnit.Framework;

namespace Server.Tests
{
  public class MiniRedisTests
  {
    private MiniRedis _redis;
    
    [SetUp]
    public void Setup()
    {
      _redis = new MiniRedis();
    }

    [Test]
    public void WhenSetValueByKey_AndGetThisValueByThisKey_ThenValueIsTheSame()
    {
      // Setup
      const string key = "my-key";
      const string value = "my-value";
      
      // Do
      _redis.ProcessCommand($"SET {key} {value}");
      
      // Check
      var response = _redis.ProcessCommand($"GET {key}");
      Assert.AreEqual(value, response.Result);
    }
    
    [Test]
    public void WhenSetValueByKeyWithExpiration_AndGetThisValueByThisKey_ThenValueIsReturnedOnlyWhenNotExpired()
    {
      // Setup
      const string key = "my-key";
      const string value = "my-value";
      const int expirationSeconds = 5;
      
      // Do
      _redis.ProcessCommand($"SET {key} {value} {expirationSeconds}");
      
      // Check
      var response = _redis.ProcessCommand($"GET {key}");
      Assert.AreEqual(value, response.Result);
      
      Thread.Sleep(TimeSpan.FromSeconds(expirationSeconds));
      
      response = _redis.ProcessCommand($"GET {key}");
      Assert.AreEqual(response.Result, "(nil)");
    }
    
    [Test]
    public void WhenSetValueByKey_AndDeleteThisValueByThisKey_ThenNilIsReturnedByThisKey()
    {
      // Setup
      const string key = "my-key";
      const string value = "my-value";
      
      // Do
      _redis.ProcessCommand($"SET {key} {value}");
      
      // Check
      var deletedResponse = _redis.ProcessCommand($"DEL {key}");
      var storedValueResponse = _redis.ProcessCommand($"GET {key}");
      
      Assert.AreEqual(deletedResponse.Result, "1");
      Assert.AreEqual(storedValueResponse.Result, "(nil)");
    }
    
    [Test]
    public void WhenAddFiveKeys_AndGetDbSize_ThenDbSizeIsFive()
    {
      // Setup
      _redis.ProcessCommand($"SET key1 value1");
      _redis.ProcessCommand($"SET key2 value2");
      _redis.ProcessCommand($"ZADD key3 10 user1");
      _redis.ProcessCommand($"ZADD key3 11 user2");
      _redis.ProcessCommand($"ZADD key4 10 user3");
      _redis.ProcessCommand($"ZADD key4 15 user4");
      _redis.ProcessCommand($"ZADD key5 15 user5");
      
      // Do
      var response = _redis.ProcessCommand($"DBSIZE");
      
      // Check
      Assert.AreEqual("5", response.Result);
    }
    
    [Test]
    public void WhenSetValueByKey_AndIncrementThisValue_ThenValueIsIncremented()
    {
      // Setup
      const string key = "my-key";
      const int value = 5;
      _redis.ProcessCommand($"SET {key} {value}");
      
      // Do
      _redis.ProcessCommand($"INCR {key}");
      
      // Check
      var response = _redis.ProcessCommand($"GET {key}");
      
      Assert.AreEqual((5 + 1).ToString(), response.Result);
    }
    
    [Test]
    public void WhenSetSortedSetValueByKey_AndIncrementThisValue_ThenErrorIsReturned()
    {
      // Setup
      const string key = "my-key";
      const double score = 10;
      const int value = 5;
      
      _redis.ProcessCommand($"ZADD {key} {score} {value}");
      
      // Do
      var response = _redis.ProcessCommand($"INCR {key}");
      
      // Check
      Assert.IsTrue(response.Error?.Code == MiniRedisErrorCode.InvalidOperation);
    }
    
    [Test]
    public void WhenSetThreeSortedSetValuesByKey_AndGetCardinality_ThenThreeIsReturned()
    {
      // Setup
      const string key = "my-key";
      
      _redis.ProcessCommand($"ZADD {key} 1 user1");
      _redis.ProcessCommand($"ZADD {key} 2 user2");
      _redis.ProcessCommand($"ZADD {key} 3 user3");
      _redis.ProcessCommand($"ZADD {key}2 3 user6");
      _redis.ProcessCommand($"ZADD {key}2 1 user7");
      
      // Do
      var response = _redis.ProcessCommand($"ZCARD {key}");
      
      // Check
      Assert.AreEqual("3", response.Result);
    }
    
    [Test]
    public void WhenAddThreeSortedSetValuesByKey_AndGetRankOfSecondInOrder_ThenIndexOneIsReturned()
    {
      // Setup
      const string key = "my-key";
      
      _redis.ProcessCommand($"ZADD {key} 3 user3");
      _redis.ProcessCommand($"ZADD {key} 1 user1");
      _redis.ProcessCommand($"ZADD {key} 2 user2");
      
      // Do
      var response = _redis.ProcessCommand($"ZRANK {key} user2");
      
      // Check
      Assert.AreEqual("1", response.Result);
    }
    
    [Test]
    public void WhenAddThreeSortedSetValuesByKey_AndAddingThenInOneCommand_ThenThreeValuesAreStored()
    {
      // Setup
      const string key = "my-key";
      _redis.ProcessCommand($"ZADD {key} 3 user3 1 user1 2 user2");
      
      // Do
      var response = _redis.ProcessCommand($"ZRANGE {key} 0 -1");
      
      // Check
      Assert.AreEqual(3, response.Result?.Split(Environment.NewLine).Length);
    }
    
    [Test]
    public void WhenSetFourSortedSetValuesByKey_AndGetRange_ThenRequestedRangeIsReturned()
    {
      // Setup
      const string key = "my-key";
      string[] values  = { "user1", "user2", "user3", "user4" };
      
      _redis.ProcessCommand($"ZADD {key} 1 {values[0]}");
      _redis.ProcessCommand($"ZADD {key} 2 {values[1]}");
      _redis.ProcessCommand($"ZADD {key} 3 {values[2]}");
      _redis.ProcessCommand($"ZADD {key} 4 {values[3]}");
      
      // Do
      var firstResponse = _redis.ProcessCommand($"ZRANGE {key} 1 3");
      var secondResponse = _redis.ProcessCommand($"ZRANGE {key} 1 -2");
      
      // Check
      Assert.AreEqual(string.Join(Environment.NewLine, values.Skip(1).Take(3)), firstResponse.Result);
      Assert.AreEqual(string.Join(Environment.NewLine, values.Skip(1).Take(2)), secondResponse.Result);
    }
    
    [Test]
    public void WhenSetFourSortedSetValuesByKey_AndTwoHaveEqualScore_ThenFourValuesAreStoredOrderByScore()
    {
      // Setup
      const string key = "my-key";
      string[] values  = { "user1", "user2", "user3", "user4" };

      _redis.ProcessCommand($"ZADD {key} 2 {values[2]}");
      _redis.ProcessCommand($"ZADD {key} 1 {values[0]}");
      _redis.ProcessCommand($"ZADD {key} 2 {values[1]}");
      _redis.ProcessCommand($"ZADD {key} 3 {values[3]}");
      
      // Do
      var response = _redis.ProcessCommand($"ZRANGE {key} 0 -1");
      
      // Check
      Assert.AreEqual(string.Join(Environment.NewLine, values), response.Result);
    }
  }
}