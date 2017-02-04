using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
  public interface IParticle:IActor
  {
    Task DestermineLocation(double x, double y, Guid aggreator);
  }
}
