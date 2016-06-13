using System;
using System.Xml;

namespace CritterShell.Gpx
{
    internal class Metadata : XmlSerializable
    {
        public Bounds Bounds { get; private set; }
        public Link Link { get; private set; }
        public DateTime Time { get; private set; }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.Gpx.Bounds, Constant.Gpx.Namespace))
            {
                this.Bounds = new Bounds();
                this.Bounds.ReadXml(reader);
            }
            else if (reader.IsStartElement(Constant.Gpx.Link, Constant.Gpx.Namespace))
            {
                this.Link = new Link();
                this.Link.ReadXml(reader);
            }
            else if (reader.IsStartElement(Constant.Gpx.Time, Constant.Gpx.Namespace))
            {
                this.Time = reader.ReadElementContentAsDateTime();
            }
            else
            {
                reader.Read();
            }
        }
    }
}
