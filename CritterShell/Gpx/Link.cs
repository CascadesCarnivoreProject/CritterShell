using System.Xml;

namespace CritterShell.Gpx
{
    internal class Link : XmlSerializable
    {
        public string Href { get; private set; }
        public string Text { get; private set; }

        protected override void OnStartElement(XmlReader reader)
        {
            if (reader.IsStartElement(Constant.Gpx.Link, Constant.Gpx.Namespace))
            {
                if (reader.MoveToAttribute(Constant.Gpx.Href))
                {
                    this.Href = reader.Value;
                }
                reader.Read();
            }
            else if (reader.IsStartElement(Constant.Gpx.Text, Constant.Gpx.Namespace))
            {
                this.Text = reader.ReadElementContentAsString();
            }
            else
            {
                reader.Read();
            }
        }
    }
}