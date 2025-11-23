using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Collections.Specialized.BitVector32;

namespace BackendBiking
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service1 : IService1
	{
        private readonly ProxyBikeSOAP.Service1Client clientSoap;
        public Service1() 
        {
           clientSoap =  new ProxyBikeSOAP.Service1Client();
        }

		public string GetData(int value)
		{
			return string.Format("You entered: {0}", value);
		}
        public async Task<String> GetAdresse(string adresse)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8080");
            String addr = await clientSoap.getAdresseAsync(adresse);
            return addr;
        }

        public async Task<String> GetCoordonnees(string adresse)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8080");
            String addr = await clientSoap.GetCoordoneesAsync(adresse);
            return addr;
        }


        private async Task<Position> GetPosition(string stringPos)
        {
            String posResponse = await clientSoap.GetCoordoneesAsync(stringPos);
            Position pos = JsonConvert.DeserializeObject<Position>(posResponse);

            return pos;
        }

        private double GetDuration(string routeResponse)
        {
            JObject routeObject = JObject.Parse(routeResponse);
            double duration = (double)routeObject["features"][0]["properties"]["segments"][0]["duration"];

            return duration;
        }
        static string ExtraireVille(string adresse)
        {
            // Cherche un code postal suivi du nom de la ville
            var match = Regex.Match(adresse, @"\b\d{5}\s+(.+)$");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return string.Empty; // Aucun résultat trouvé
        }



        public async Task<string> GetRoute(string start, string dest)
		{
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "http://localhost:8080");
            try
            {
                var route = new Route();

                Position posStart = await GetPosition(start);
                Position posDest = await GetPosition(dest);

                //foot-walking mode only
                string walkingRouteResponse = await clientSoap.getParcoursAsync(posStart.lat, posStart.lng, posDest.lat, posDest.lng, false);
                double walkingOnlyDuration = GetDuration(walkingRouteResponse);
                
                //cycling-regular mode
                string startStationsResponse = await clientSoap.GetContractAsync(ExtraireVille(start));
                string destStationsResponse = await clientSoap.GetContractAsync(ExtraireVille(dest));
                
                List<Station> startStations = JsonConvert.DeserializeObject<List<Station>>(startStationsResponse);
                List<Station> destStations = JsonConvert.DeserializeObject<List<Station>>(destStationsResponse);
                if (startStations != null && destStations != null && startStations.Count != 0 && destStations.Count != 0)
                {
                    Station startClosestStation = Station.getClosestStation(posStart, startStations);
                    Station destClosestStation = Station.getClosestStation(posDest, destStations);

                    Position posStartStat = startClosestStation.position;
                    Position posDestStat = destClosestStation.position;

                    string cyclingRouteResponse = await clientSoap.getParcoursAsync(posStartStat.lat, posStartStat.lng, posDestStat.lat, posDestStat.lng, true);
                    string walkingRouteResponse1 = await clientSoap.getParcoursAsync(posStart.lat, posStart.lng, posStartStat.lat, posStartStat.lng, false);
                    string walkingRouteResponse2 = await clientSoap.getParcoursAsync(posDestStat.lat, posDestStat.lng, posDest.lat, posDest.lng, false);

                    double cyclingDuration = (double)GetDuration(cyclingRouteResponse);
                    double walkingDuration1 = (double)GetDuration(walkingRouteResponse1);
                    double walkingDuration2 = (double)GetDuration(walkingRouteResponse2);
                    double totalDuration = walkingDuration1 + cyclingDuration + walkingDuration2;

                    if (totalDuration < walkingOnlyDuration)
                    {
                        
                        route.segments.Add(new RouteSegment { mode = "walking", route = JObject.Parse(walkingRouteResponse1), duration = walkingDuration1 });
                        route.segments.Add(new RouteSegment { mode = "cycling", route = JObject.Parse(cyclingRouteResponse), duration = cyclingDuration });
                        route.segments.Add(new RouteSegment { mode = "walking", route = JObject.Parse(walkingRouteResponse2), duration = walkingDuration2 });

                        return JsonConvert.SerializeObject(route);
                    }
                    else return JsonConvert.SerializeObject(route);
                }
                else
                {
                    route.segments.Add(new RouteSegment { mode = "walking", route = JObject.Parse(walkingRouteResponse), duration = walkingOnlyDuration });

                    return JsonConvert.SerializeObject(route);
                }

                    
            }
            catch (Exception ex)
            {
                var errorJson = JsonConvert.SerializeObject(new
                {
                    error = ex.Message,
                    stack = ex.StackTrace
                });
                return errorJson;
            }
		    }

            }

    public class Position
    {

        public double lat { get; set; }
        public double lng { get; set; }

        public override string ToString()
        {
            return $"lat : {lat}" +
                   $"lng : {lng}\n";
        }

    }
    
   }
