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
        public string mode { get; set; }  // "walking" ou "cycling"
        public JObject route { get; set; } // JSON renvoyé par getParcoursAsync
        public double duration { get; set; } // durée en secondes
    }
}
