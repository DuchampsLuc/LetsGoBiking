        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Net;
        using System.Net.Http;
        using System.Text;
        using System.Threading.Tasks;
        using static System.Net.WebRequestMethods;
        using Newtonsoft.Json;
        using System.Collections.Generic;
        using System.Device.Location;
        using static System.Collections.Specialized.BitVector32;
        using System.Xml.Linq;
using System.Globalization;


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

                    String responseBody1 = await clientSoap.GetCoordoneesAsync("Paris");
                    Console.WriteLine(responseBody1);
                    String responseBody2 = await clientSoap.GetCoordoneesAsync("Lyon");
                    Console.WriteLine(responseBody2);
                    Position pos1 = JsonConvert.DeserializeObject<Position>(responseBody1);
                    Position pos2 = JsonConvert.DeserializeObject<Position>(responseBody2);
                    Console.WriteLine(pos1);
                    Console.WriteLine(pos2);
                    
                    String responseBody = await clientSoap.getParcoursAsync(pos1.lat, pos1.lng, pos2.lat, pos2.lng);
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
                Station closeststation = null;
                GeoCoordinate mygeo = new GeoCoordinate(station.position.lat, station.position.lng);
                GeoCoordinate closest = null;
            for (int i = 0; i < stations.Count; i++)
            {
                GeoCoordinate temp = new GeoCoordinate(stations[i].position.lat, stations[i].position.lng);
                if (mygeo.GetDistanceTo(temp) != 0) {
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
                        closest= new GeoCoordinate(stations[i].position.lat, stations[i].position.lng);
                    }
                }
            }
                return closeststation;

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
