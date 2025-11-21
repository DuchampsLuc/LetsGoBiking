using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendBiking
{
    public class Route
    {
        public List<RouteSegment> segments { get; set; } = new List<RouteSegment>();
    }

    public class RouteSegment
    {
        public string mode { get; set; }  
        public JObject route { get; set; } 
        public double duration { get; set; }
    }
}
