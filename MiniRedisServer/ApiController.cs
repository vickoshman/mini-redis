using Microsoft.AspNetCore.Mvc;
using MiniRedisCore;
using MiniRedisCore.Communication;

namespace MiniRedisServer
{
  [Route("/")]
  [ApiController]
  public class ApiController : ControllerBase
  {
    private static readonly MiniRedis Redis = new MiniRedis();
    
    [HttpGet("cmd={command}")]
    public IActionResult Index(string command)
    {
      return ApiResponse(Redis.ProcessCommand(command));
    }
    
    [HttpGet("{key}")]
    public IActionResult Get(string key)
    {
      return ApiResponse(Redis.ProcessCommand($"GET {key}"));
    }

    [HttpPut("SET")]
    public IActionResult Set(ApiRequestDto dto)
    {
      return ApiResponse(Redis.ProcessCommand($"SET {dto.Params}"));
    }
    
    [HttpDelete("DEL")]
    public IActionResult Del(ApiRequestDto dto)
    {
      return ApiResponse(Redis.ProcessCommand($"DEL {dto.Params}"));
    }
    
    [HttpGet("DBSIZE")]
    public IActionResult DbSize()
    {
      return ApiResponse(Redis.ProcessCommand("DBSIZE"));
    }
    
    [HttpPost("INCR")]
    public IActionResult Incr(ApiRequestDto dto)
    {
      return ApiResponse(Redis.ProcessCommand($"INCR {dto.Params}"));
    }
    
    [HttpPost("ZADD")]
    public IActionResult ZAdd(ApiRequestDto dto)
    {
      return ApiResponse(Redis.ProcessCommand($"ZADD {dto.Params}"));
    }
    
    [HttpGet("ZCARD/{key}")]
    public IActionResult ZCard(string key)
    {
      return ApiResponse(Redis.ProcessCommand($"ZCARD {key}"));
    }
    
    [HttpGet("ZRANK/{key}")]
    public IActionResult ZRank(string key)
    {
      return ApiResponse(Redis.ProcessCommand($"ZRANK {key}"));
    }
    
    [HttpGet("ZRANGE/{key}/{start}/{stop}")]
    public IActionResult ZRange(string key, string start, string stop)
    {
      return ApiResponse(Redis.ProcessCommand($"ZRANGE {key} {start} {stop}"));
    }
    
    private IActionResult ApiResponse(MiniRedisResponse redisResponse)
    {
      if (redisResponse.Error != null)
        return BadRequest(redisResponse.Error.ToString());
      
      return Ok(redisResponse.Result);
    }
  }
}