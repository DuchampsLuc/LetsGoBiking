using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendBiking
{
    internal class Station
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

        public static Station getClosestStation(Position pos, List<Station> stations)
        {
            Station closeststation = null;
            GeoCoordinate mygeo = new GeoCoordinate(pos.lat, pos.lng);
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
                        closeststation = stations[i];
                    }
                }
            }
            return closeststation;

        }
    }

}
