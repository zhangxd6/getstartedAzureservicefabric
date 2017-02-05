using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
  public interface IAggreator:IActor
  {
    Task Report(bool status, long index);
    Task<Reslut> GetResult();
    Task Init(long count);
  }
}
