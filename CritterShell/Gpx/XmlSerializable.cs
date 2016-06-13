using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CritterShell.Gpx
{
    internal abstract class XmlSerializable : IXmlSerializable
    {
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool isEmptyElement = reader.IsEmptyElement;
            using (XmlReader subReader = reader.ReadSubtree())
            {
                while (subReader.EOF == false)
                {
                    if (subReader.IsStartElement())
                    {
                        this.OnStartElement(subReader);
                    }
                    else
                    {
                        subReader.Read();
                    }
                }
            }
            if (isEmptyElement)
            {
                reader.Read();
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        protected abstract void OnStartElement(XmlReader reader);
    }
}
