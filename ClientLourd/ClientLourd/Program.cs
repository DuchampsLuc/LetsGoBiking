using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClientLourd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ItineraryServiceReference.Service1Client itineraryService = new ItineraryServiceReference.Service1Client();

            Console.WriteLine("Choisissez votre point de départ");
            String start = Console.ReadLine();
            Console.WriteLine("Choisissez votre point d'arrivée");
            String dest = Console.ReadLine();

            String jsonResult = itineraryService.GetRoute(start, dest);

            JObject routeObject = JObject.Parse(jsonResult);

            foreach (var segment in routeObject["segments"])
            {
                string mode = segment["mode"].ToString();

                var steps = segment["route"]["features"][0]["properties"]["segments"][0]["steps"];

                if (mode == "cycling")
                {
                    Console.WriteLine("Prendre le vélo");
                }

                foreach (var step in steps)
                {
                    string instruction = step["instruction"].ToString();
                    Console.WriteLine(instruction);
                }

                if (mode == "cycling")
                {
                    Console.WriteLine("Déposer le vélo");
                }

                Console.WriteLine(); // Pour séparer les segments
            }

        }
    }
}
