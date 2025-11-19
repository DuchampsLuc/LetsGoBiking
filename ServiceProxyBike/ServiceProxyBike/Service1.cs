using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using static System.Collections.Specialized.BitVector32;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

namespace ServiceProxyBike
{
	// REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
	public class Service1 : IService1
	{
        static readonly HttpClient client = new HttpClient();
        static GenericProxyCache<string> cache = new GenericProxyCache<string>();
        static GenericProxyCache<Position> CoorCache = new GenericProxyCache<Position>();
        static GenericProxyCache<Parcours> ParcoursCache = new GenericProxyCache<Parcours>();

        public async Task<string> GetContract(string contract)
		{     
            try
            {
                if (cache.Get(contract,20.0) == null)
                {
                    String Path = "https://api.jcdecaux.com/vls/v1/stations?apiKey=a642c1e64227fd4b746cef12768dbb982a375e77";
                    Path = Path + "&contract=" + contract;
                    HttpResponseMessage response = await client.GetAsync(Path);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);
                    cache.addValue(contract, responseBody);
                    return responseBody;
                }
                else
                {

                    return cache.Get(contract);
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "";
            }
        }
        public async Task<string> GetCoordonees(string adresse)
        {
            try
            {
                if (CoorCache.Get(adresse, 86400) == null)
                {
                    string url = $"https://api-adresse.data.gouv.fr/search/?q={Uri.EscapeDataString(adresse)}&limit=1";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);
                    JObject obj = JObject.Parse(json);
                    var feature = obj["features"]?.FirstOrDefault();
                    var coords = feature["geometry"]["coordinates"];
                    double longitude = coords[0].Value<double>();
                    double latitude = coords[1].Value<double>();
                    Position pos = new Position(latitude, longitude);
                    CoorCache.addValue(adresse,pos);

                    return JsonConvert.SerializeObject(pos);
                }
                else
                {

                    return JsonConvert.SerializeObject(CoorCache.Get(adresse));
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "";
            }
        }

        public async Task<string> getParcours(double lat1, double lng1, double lat2, double lng2)
        {
            Position pos1 = new Position(lat1, lng1);
            Position pos2 = new Position(lat2, lng2);
            String PourCache = JsonConvert.SerializeObject(pos1) + JsonConvert.SerializeObject(pos2);
            
            try
            {
                if (ParcoursCache.Get(PourCache, 86400) == default(Parcours))
                {

                    string apiKey = "eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjVjYjFmYjIxYWQ1YzQxNjdiMzdhOGFkMGQyNGQxZDUyIiwiaCI6Im11cm11cjY0In0="; // remplace par ta clé ORS
                    //string url = $"https://api.openrouteservice.org/v2/directions/foot-walking?api_key={apiKey}&start={pos1.lng.ToString(CultureInfo.InvariantCulture)},{pos1.lat.ToString(CultureInfo.InvariantCulture)}&end={pos2.lng.ToString(CultureInfo.InvariantCulture)},{pos2.lat.ToString(CultureInfo.InvariantCulture)}";
             
                    string url = $"https://api.openrouteservice.org/v2/directions/foot-walking?api_key={Uri.EscapeDataString(apiKey)}&start={pos1.lng.ToString(CultureInfo.InvariantCulture)},{pos1.lat.ToString(CultureInfo.InvariantCulture)}&end={pos2.lng.ToString(CultureInfo.InvariantCulture)},{pos2.lat.ToString(CultureInfo.InvariantCulture)}";
                   
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    
                    string json = await response.Content.ReadAsStringAsync();
                    
                    JObject obj = JObject.Parse(json);

                    
                    Parcours parcours = new Parcours();
                    var steps = obj["features"]?[0]?["properties"]?["segments"]?[0]?["steps"];
                   
                    if (steps != null)
                    {
                        foreach (var step in steps)
                        {
                            var start = step["start_location"] ?? step["way_points"]?[0];
                            var end = step["end_location"] ?? step["way_points"]?[1];

                            double lat = start?[0]?.Value<double>() ?? 0;
                            double lng = start?[1]?.Value<double>() ?? 0;
                            string instruction = step["instruction"]?.Value<string>() ?? "";

                            parcours.Etapes.Add(new Etape
                            {
                                Position = new Position (lat, lng),
                                Instruction = instruction
                            });
                        }
                        return "test";
                        ParcoursCache.addValue(PourCache, parcours);
                        
                    }

                    String test= JsonConvert.SerializeObject(parcours);
                    return "t";
                }
                else
                {

                    //return JsonConvert.SerializeObject(ParcoursCache.Get(PourCache));
                    return "testnbihjv";
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "jngsjbgkjkjgdf";
            }
       
        }


    }
    public class GenericProxyCache<T>
    {
        Dictionary<string,T> cache = new Dictionary<string,T>();
        Dictionary<string, DateTimeOffset> date = new Dictionary<string,DateTimeOffset>();
        public DateTimeOffset dt_default = ObjectCache.InfiniteAbsoluteExpiration;

        public T Get(String contract)
        {
            try
            {
                var temp = cache[contract];
                return temp;
            }

            catch(KeyNotFoundException e)
            {
                try
                {
                    date.Add(contract, dt_default);
                    return default(T);
                }
                catch
                {
                    return default(T);
                }     
            }
        }

        public void addValue(String contract,T value)
        {
            cache.Add(contract, value);
        }
        public void removeValue(String contract)
        {
            cache.Remove(contract);
            date.Remove(contract);
        }
        public T Get(string CacheItemName, double dt_seconds)
        {
            try
            {
                var temp = cache[CacheItemName];
                if (date[CacheItemName] > DateTimeOffset.Now)
                {
                    return temp;
                }
                else
                {
                    date.Remove(CacheItemName);
                    date.Add(CacheItemName, DateTimeOffset.Now.AddSeconds(dt_seconds));
                    cache.Remove(CacheItemName);
                    return default(T);
                }
            }
            catch (KeyNotFoundException e)
            {  
               date.Add(CacheItemName, DateTimeOffset.Now.AddSeconds(dt_seconds));
               return default(T);
            }

        }
        public T Get(string CacheItemName, DateTimeOffset dt)
        {
            try
            {
                var temp = cache[CacheItemName];
                if (date[CacheItemName] > DateTimeOffset.Now)
                {
                    date.Remove(CacheItemName);
                    date.Add(CacheItemName, dt);
                    return temp;
                }
                else
                {
                    date.Remove(CacheItemName);
                    date.Add(CacheItemName, dt);
                    cache.Remove(CacheItemName);
                    return default(T);
                }
            }

            catch (KeyNotFoundException e)
            {
                date.Add(CacheItemName, dt);
                return default(T);
            }

        }

    }
    public class Position
    {
        public Position(double lat1,double lng1)
        {
            lat = lat1;
            lng = lng1;
                
        }
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Parcours
    {
        public List<Etape> Etapes { get; set; }
    }

    public class Etape
    {
        public Position Position { get; set; }
        public string Instruction { get; set; }
    }

}

