using DesiredState.Common;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DesiredState.IIS
{

    internal class WebConfigEntry
    {
        internal string SiteName { get; set; }
        internal string Path { get; set; }

        internal string Filter { get; set; }
        internal string Name { get; set; }
        internal IEnumerable<XAttribute> Attributes { get; set; }

        public WebConfigEntry(string name, string filter, IEnumerable<XAttribute> attributes, string path)
        {
            this.SiteName = path.Split('/')[0];
            this.Path = path;
            this.Filter = filter;
            this.Name = name;
            this.Attributes = attributes;
        }


    }

}
