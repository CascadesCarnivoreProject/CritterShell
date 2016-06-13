using System;
using System.Collections.Generic;
using System.Xml;

namespace CritterShell.Gpx
{
    internal class Extensions : XmlSerializable
    {
        public Extensions()
        {
            this.Categories = new List<string>();
        }

        public List<string> Categories { get; private set; }

        public DateTime CreationTime { get; private set; }

        public string DisplayMode { get; private set; }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.GarminExtension.Category, Constant.GarminNamespace.Waypoint1))
            {
                this.Categories.Add(reader.ReadElementContentAsString());
            }
            if (reader.IsStartElement(Constant.GarminExtension.CreationTime, Constant.GarminNamespace.CreationTime1))
            {
                this.CreationTime = reader.ReadElementContentAsDateTime();
            }
            else if (reader.IsStartElement(Constant.GarminExtension.DisplayMode, Constant.GarminNamespace.Waypoint1))
            {
                this.DisplayMode = reader.ReadElementContentAsString();
            }
            else
            {
                reader.Read();
            }
        }
    }
}