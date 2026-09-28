// Ignore Spelling: coord

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.GeneralFunctions
{
    public class MapPositions
    {
        public class GeoDistanceCalculator
        {
            public class Coordinates
            {
                public double Latitude { get; set; }
                public double Longitude { get; set; }
            }

            public static double CalculateDistance(Coordinates coord1, Coordinates coord2)
            {
                const double EarthRadiusKm = 6371.0;

                double dLat = ToRadians(coord2.Latitude - coord1.Latitude);
                double dLon = ToRadians(coord2.Longitude - coord1.Longitude);

                double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                           Math.Cos(ToRadians(coord1.Latitude)) * Math.Cos(ToRadians(coord2.Latitude)) *
                           Math.Pow(Math.Sin(dLon / 2), 2);

                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return EarthRadiusKm * c;
            }

            private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
        }
    }
}
