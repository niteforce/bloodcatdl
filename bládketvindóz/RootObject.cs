using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace bládketvindóz
{
    public class RootObject
    {

        public string synced { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string titleU { get; set; }
        public string artist { get; set; }
        public string artistU { get; set; }
        public string creatorId { get; set; }
        public string creator { get; set; }
        public string id { get; set; }
        public double maximumAr { get; set; }
        public double minimumAr { get; set; }

        public List<Beatmap> beatmaps { get; set; }

 
    }
    
}
