using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoAreasUpdate
{
    public class Update
    {
        public DateTime Record_creation_time { get; set; }
        public DateTime Record_update_time { get; set; }
        public String Code { get; set; }
        public String Description { get; set; }
        public int Type_id { get; set; } = 1;
        public String Latitude_min { get; set; }
        public String Longitude_min { get; set; }
        public String Latitude_max { get; set; }
        public String Longitude_max { get; set; }
        public Int32 Point_number { get; set; }
        public String Polygon { get; set; }
        public DateTime Last_update_time { get; set; }
        public String External_code { get; set; } = "";
        public String Color { get; set; }

    }
}
