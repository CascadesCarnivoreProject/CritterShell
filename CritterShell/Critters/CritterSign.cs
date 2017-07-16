using CritterShell.Gpx;
using System;

namespace CritterShell.Critters
{
    internal class CritterSign : CritterWithMergeableProperties
    {
        public string Description { get; private set; }
        public double Elevation { get; private set; }
        public string Identification { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public CritterSignType Type { get; private set; }

        public CritterSign(Waypoint waypoint)
        {
            this.Description = waypoint.Description;
            this.Elevation = waypoint.Elevation;
            if (waypoint.Extensions != null)
            {
                foreach (string category in waypoint.Extensions.Categories)
                {
                    this.Identification = this.MergeString(this.Identification, category);
                }
            }
            this.Latitude = waypoint.Latitude;
            this.Longitude = waypoint.Longitude;
            this.Name = waypoint.Name;
            this.Time = waypoint.Time;

            CritterSignType type;
            waypoint.TryGetSignType(out type);
            this.Type = type;
        }
    }
}
