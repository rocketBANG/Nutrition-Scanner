using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutrition_Scanner
{
    public class NutrientModel
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "Nutrient")]
        public string Nutrient { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime Date { get; set; }
    }
}
