using System.Collections.Generic;
using System.Xml.Linq;

namespace Weatherlog.Models
{
    static class Extensions
    {
        public static XDocument ToXDocument(this XElement element, string rootName, XAttribute[] attributes = null)
        {
            return ToXDocument(new XElement[] { element }, rootName, attributes);
        }

        public static XDocument ToXDocument(this IEnumerable<XElement> elements, string rootName, XAttribute[] attributes = null)
        {
            var root = new XElement(rootName);
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    root.Add(attr);
                }
            }
            foreach (var item in elements)
            {
                root.Add(item);
            }

            return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
        }
    }
}
