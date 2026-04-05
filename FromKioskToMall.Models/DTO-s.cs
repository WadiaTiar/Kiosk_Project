using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FromKioskToMall.Models
{
    public class KioskItem
    {
        public Guid ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Metadata { get; set; } // نص الـ JSON
        public DateTime CreatedAt { get; set; }

        public string ProductCategory { get; set; }

        // وظيفة لتحويل الـ Metadata إلى قيم برمجية
        public Dictionary<string, object> GetProperties()
        {
            if (string.IsNullOrEmpty(Metadata)) return new Dictionary<string, object>();
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(Metadata);
        }
        public decimal GetPrice()
        {
            var props = GetProperties();
            if (props.ContainsKey("price"))
                return Convert.ToDecimal(props["price"]);
            return 0;
        }
    }
}
