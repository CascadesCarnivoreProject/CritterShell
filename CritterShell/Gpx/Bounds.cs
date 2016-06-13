using System;
using System.Xml;

namespace CritterShell.Gpx
{
    internal class Bounds : XmlSerializable
    {
        public double MaximumLatitude { get; private set; }
        public double MaximumLongitude { get; private set; }
        public double MinimumLatitude { get; private set; }
        public double MinimumLongitude { get; private set; }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.Gpx.Bounds, Constant.Gpx.Namespace))
            {
                if (reader.MoveToAttribute(Constant.Gpx.MaximumLatitude))
                {
                    this.MaximumLatitude = Double.Parse(reader.Value);
                }
                if (reader.MoveToAttribute(Constant.Gpx.MaximumLongitude))
                {
                    this.MaximumLongitude = Double.Parse(reader.Value);
                }
                if (reader.MoveToAttribute(Constant.Gpx.MinimumLatitude))
                {
                    this.MinimumLatitude = Double.Parse(reader.Value);
                }
                if (reader.MoveToAttribute(Constant.Gpx.MinimumLongitude))
                {
                    this.MinimumLongitude = Double.Parse(reader.Value);
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