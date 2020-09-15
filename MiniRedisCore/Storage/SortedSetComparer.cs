using System;
using System.Collections.Generic;

namespace MiniRedisCore.Storage
{
  public class SortedSetComparer : IComparer<double>
  {
    public int Compare(double x, double y)
    {
      if (Math.Abs(x - y) < double.Epsilon)
        return 1;
      
      return (int) (x - y);
    }
  }
}