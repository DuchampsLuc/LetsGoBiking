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
        static GenericProxyCache<string> ParcoursCache = new GenericProxyCache<string>();

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

        public async Task<string> getParcours(double lat1, double lng1, double lat2, double lng2, bool isCycling)
        {
            Position pos1 = new Position(lat1, lng1);
            Position pos2 = new Position(lat2, lng2);
            string PourCache = JsonConvert.SerializeObject(pos1) + JsonConvert.SerializeObject(pos2) + JsonConvert.SerializeObject(isCycling);

            try
            {
                if (ParcoursCache.Get(PourCache, 86400) == null)
                {

                    string apiKey = "eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjVjYjFmYjIxYWQ1YzQxNjdiMzdhOGFkMGQyNGQxZDUyIiwiaCI6Im11cm11cjY0In0=";

                    string travelMode;
                    if (!isCycling) travelMode = "foot-walking";
                    else travelMode = "cycling-regular";

                    string url = $"https://api.openrouteservice.org/v2/directions/" + travelMode + $"?api_key={apiKey}&start={pos1.lng.ToString(CultureInfo.InvariantCulture)},{pos1.lat.ToString(CultureInfo.InvariantCulture)}&end={pos2.lng.ToString(CultureInfo.InvariantCulture)},{pos2.lat.ToString(CultureInfo.InvariantCulture)}";

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    ParcoursCache.addValue(PourCache, json);

                    return json;
                }
                else
                {

                    return ParcoursCache.Get(PourCache);

                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "";
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
    
}

