using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CritterShell.Gpx
{
    internal class GpxFile : XmlSerializable
    {
        public GpxFile(string gpxFilePath)
        {
            this.Waypoints = new List<Waypoint>();

            using (FileStream stream = new FileStream(gpxFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    reader.MoveToContent();
                    this.ReadXml(reader);
                }
            }
        }

        public string Creator { get; private set; }

        public Metadata Metadata { get; private set; }

        public List<Waypoint> Waypoints { get; private set; }

        public string Version { get; private set; }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.Gpx.GpxElement, Constant.Gpx.Namespace))
            {
                if (reader.MoveToAttribute(Constant.Gpx.Creator))
                {
                    this.Creator = reader.Value;
                }
                if (reader.MoveToAttribute(Constant.Gpx.Version))
                {
                    this.Version = reader.Value;
                }

                reader.Read();
            }
            else if (reader.IsStartElement(Constant.Gpx.Metadata, Constant.Gpx.Namespace))
            {
                this.Metadata = new Metadata();
                this.Metadata.ReadXml(reader);
            }
            else if (reader.IsStartElement(Constant.Gpx.Waypoint, Constant.Gpx.Namespace))
            {
                Waypoint waypoint = new Waypoint();
                waypoint.ReadXml(reader);
                this.Waypoints.Add(waypoint);
            }
            else
            {
                reader.Read();
            }
        }
    }
}
