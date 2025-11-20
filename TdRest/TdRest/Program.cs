        using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
        using System;
        using System.Collections.Generic;
        using System.Collections.Generic;
        using System.Device.Location;
using System.Diagnostics.Contracts;
using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Net;
        using System.Net.Http;
        using System.Text;
        using System.Threading.Tasks;
        using System.Xml.Linq;
        using static System.Collections.Specialized.BitVector32;
        using static System.Net.WebRequestMethods;


namespace TdRest
    {
        internal class Program
        {
            static readonly HttpClient client = new HttpClient();
            static async Task Main(string[] args)
            {
                try
                {
                    Console.WriteLine("Met ta ville sous contract");
                    String contract = Console.ReadLine();
                    var clientSoap = new ProxyBikeSOAP.Service1Client();


                /*string responseBody = await clientSoap.GetContractAsync(contract);
                */

                String start = "Amiens", dest = "Lyon";
                /*
                    String responseBody1 = await clientSoap.GetCoordoneesAsync(start);
                    Console.WriteLine(responseBody1);
                    String responseBody2 = await clientSoap.GetCoordoneesAsync(dest);
                    Console.WriteLine(responseBody2);
                    Position pos1 = JsonConvert.DeserializeObject<Position>(responseBody1);
                    Position pos2 = JsonConvert.DeserializeObject<Position>(responseBody2);
                    Console.WriteLine(pos1);
                    Console.WriteLine(pos2);*/

                String responseBody = await getRoute(clientSoap, start, dest); //await clientSoap.getParcoursAsync(pos1.lat, pos1.lng, pos2.lat, pos2.lng);
                    Console.WriteLine(responseBody);
                    /*
                    Console.WriteLine("Choisis une station parmi celle qui sont affichées,rentre le nom!!");
                    String contract1 = Console.ReadLine();
                    List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
                    var stationNom = stations.Find(s => s.name == contract1);
                    Console.WriteLine(stationNom);
                    Console.WriteLine("\n");
                    Station s1 = getClosestStation(stationNom, stations);
                    Console.WriteLine(s1);*/


            }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            static Station getClosestStation(Station station,List<Station> stations)
            {
            return getClosestStation(station.position.lat, station.position.lng, stations);
            }


            static Station getClosestStation(double lat, double lng, List<Station> stations)
            {
                Station closeststation = null;
                GeoCoordinate mygeo = new GeoCoordinate(lat, lng);
                GeoCoordinate closest = null;
                for (int i = 0; i < stations.Count; i++)
                {
                    GeoCoordinate temp = new GeoCoordinate(stations[i].position.lat, stations[i].position.lng);
                    if (mygeo.GetDistanceTo(temp) != 0)
                    {
                        if (closest != null)
                        {

                            if (mygeo.GetDistanceTo(temp) < mygeo.GetDistanceTo(closest))
                            {
                                closest = new GeoCoordinate(stations[i].position.lat, stations[i].position.lng);
                                closeststation = stations[i];
                            }
                        }
                        else
                        {
                            closest = new GeoCoordinate(stations[i].position.lat, stations[i].position.lng);
                        }
                    }
                }
                return closeststation;

            }

        static async Task<string> getRoute(ProxyBikeSOAP.Service1Client clientSoap, string start, string dest)
            {
            String startResponse = await clientSoap.GetCoordoneesAsync(start);
            Console.WriteLine(startResponse);
            String destResponse = await clientSoap.GetCoordoneesAsync(dest);
            Console.WriteLine(destResponse);
            Position posStart = JsonConvert.DeserializeObject<Position>(startResponse);
            Position posDest = JsonConvert.DeserializeObject<Position>(destResponse);

            string walkingRouteResponse = await clientSoap.getParcoursAsync(posStart.lat, posStart.lng, posDest.lat, posDest.lng, false);

            //string walkingJson = JsonConvert.DeserializeObject<string>(walkingRouteResponse);
            JObject walkingObject = JObject.Parse(walkingRouteResponse);
            double walkingOnlyDuration = (double)walkingObject["features"][0]["properties"]["segments"][0]["duration"];


            string startStationsResponse = await clientSoap.GetContractAsync(start); 
            string destStationsResponse = await clientSoap.GetContractAsync(dest);

            
            List<Station> startStations = JsonConvert.DeserializeObject<List<Station>>(startStationsResponse);
            List<Station> destStations = JsonConvert.DeserializeObject<List<Station>>(destStationsResponse);
            if( startStations != null && destStations != null && startStations.Count != 0 && destStations.Count != 0)
            {
                Station startClosestStation = getClosestStation(posStart.lat, posStart.lng, startStations);
                Station destClosestStation = getClosestStation(posDest.lat, posDest.lng, destStations);

                Position posStartStat = startClosestStation.position;
                Position posDestStat = destClosestStation.position;
                string cyclingRouteResponse = await clientSoap.getParcoursAsync(posStartStat.lat, posStartStat.lng, posDestStat.lat, posDestStat.lng, true);

                string walkingRouteResponse1 = await clientSoap.getParcoursAsync(posStart.lat, posStart.lng, posStartStat.lat, posStartStat.lng, false);
                string walkingRouteResponse2 = await clientSoap.getParcoursAsync(posDestStat.lat, posDestStat.lng, posDest.lat, posDest.lng, false);

                JObject cyclingRoute = JObject.Parse(cyclingRouteResponse);
                double cyclingDuration = (double)cyclingRoute["features"][0]["properties"]["segments"][0]["duration"];

                JObject walkingRoute1 = JObject.Parse(walkingRouteResponse1);
                double walkingDuration1 = (double)walkingRoute1["features"][0]["properties"]["segments"][0]["duration"];

                JObject walkingRoute2 = JObject.Parse(walkingRouteResponse1);
                double walkingDuration2 = (double)walkingRoute1["features"][0]["properties"]["segments"][0]["duration"];

                double totalDuration = walkingDuration1 + cyclingDuration + walkingDuration2;

                if (totalDuration < walkingOnlyDuration) return walkingRouteResponse1 + cyclingRoute + walkingRouteResponse2;
            }
            

            //string cyclingRoute = ;

            return walkingRouteResponse;
            }




        }
        public class Position{

        public double lat { get; set;}
        public double lng { get; set; }

        public override string ToString()
        {
            return $"lat : {lat}" +
                   $"lng : {lng}\n";
        }

        }


        
        public class Station
        {

            public string name { get; set; }
            public int number { get; set; }
            public string contract_name { get; set; }
            public string address { get; set; }
            public Position position { get; set; }
            public bool banking { get; set; }
            public bool bonus { get; set; }
            public int bike_stands { get; set; }
            public int available_bike_stands { get; set; }
            public int available_bikes { get; set; }
            public string status { get; set; }
            public Int64 last_update { get; set; }

            public override string ToString()
            {
                return $"Station {number} - {name}\n" +
                       $"Contrat : {contract_name}\n" +
                       $"Adresse : {address}\n" +
                       $"Position : {position.lat}, {position.lng}\n" +
                       $"Banque : {banking}, Bonus : {bonus}\n" +
                       $"Capacité : {bike_stands}, Vélos libres : {available_bikes}, " +
                       $"Places libres : {available_bike_stands}\n" +
                       $"Statut : {status}\n" +
                       $"Dernière mise à jour : {last_update}";
            }


        }
    }
