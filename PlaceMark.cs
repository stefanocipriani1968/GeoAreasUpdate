using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeoAreasUpdate
{
    public class Placemark
    {
        public String Nome { get; set; }
        public String Descrizione { get; set; }
        public IEnumerable<XElement> ExtendedData { get; set; }
        public IEnumerable<XElement> Polygon { get; set; }
        public XElement Style { get; set; }
    }
}
