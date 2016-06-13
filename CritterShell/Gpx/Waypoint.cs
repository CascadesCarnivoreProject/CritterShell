using System;
using System.Xml;

namespace CritterShell.Gpx
{
    internal class Waypoint : XmlSerializable
    {
        public double Elevation { get; private set; }
        public Extensions Extensions { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string Name { get; private set; }
        public string Symbol { get; private set; }
        public DateTime Time { get; private set; }
        public string Type { get; private set; }

        public bool TryGetSignType(out CritterSignType signType)
        {
            if (this.Name != null && this.Name.Length >= 10)
            {
                // 0123456789012
                // yyyyMMddFLTnn
                switch (this.Name[10])
                {
                    case Constant.WaypointSignType.ForageSite:
                        signType = CritterSignType.ForageSite;
                        break;
                    case Constant.WaypointSignType.Photo:
                        signType = CritterSignType.Photo;
                        break;
                    case Constant.WaypointSignType.Scat:
                        signType = CritterSignType.Scat;
                        break;
                    case Constant.WaypointSignType.SubniviumAccess:
                        signType = CritterSignType.SubniviumAccess;
                        break;
                    case Constant.WaypointSignType.Track:
                        signType = CritterSignType.Track;
                        break;
                    case Constant.WaypointSignType.Urine:
                        signType = CritterSignType.Urine;
                        break;
                    default:
                        signType = CritterSignType.Unknown;
                        return false;
                }
                return true;
            }

            signType = CritterSignType.Unknown;
            return false;
        }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.Gpx.Elevation, Constant.Gpx.Namespace))
            {
                this.Elevation = reader.ReadElementContentAsDouble();
            }
            else if (reader.IsStartElement(Constant.Gpx.Extensions, Constant.Gpx.Namespace))
            {
                this.Extensions = new Extensions();
                this.Extensions.ReadXml(reader);
            }
            else if (reader.IsStartElement(Constant.Gpx.Name, Constant.Gpx.Namespace))
            {
                this.Name = reader.ReadElementContentAsString();
            }
            else if (reader.IsStartElement(Constant.Gpx.Symbol, Constant.Gpx.Namespace))
            {
                this.Symbol = reader.ReadElementContentAsString();
            }
            else if (reader.IsStartElement(Constant.Gpx.Time, Constant.Gpx.Namespace))
            {
                this.Time = reader.ReadElementContentAsDateTime();
            }
            else if (reader.IsStartElement(Constant.Gpx.Type, Constant.Gpx.Namespace))
            {
                this.Type = reader.ReadElementContentAsString();
            }
            else if (reader.IsStartElement(Constant.Gpx.Waypoint, Constant.Gpx.Namespace))
            {
                if (reader.MoveToAttribute(Constant.Gpx.Latitude))
                {
                    this.Latitude = Double.Parse(reader.Value);
                }
                if (reader.MoveToAttribute(Constant.Gpx.Longitude))
                {
                    this.Longitude = Double.Parse(reader.Value);
                }

                reader.Read();
            }
            else
            {
                reader.Read();
            }
        }
    }
}