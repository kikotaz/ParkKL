using System;
using com.parkkl.intro.Abstractions;
using Newtonsoft.Json;

namespace com.parkkl.intro.Models
{
    public class Parking : TableData
    {
        [JsonProperty(PropertyName = "Email")]
        public String Email { get; set; }

        [JsonProperty(PropertyName = "LocationName")]
        public string LocationName { get; set; }

        [JsonProperty(PropertyName = "ChargePerHour")]
        public int ChargePerHour { get; set; }

        [JsonProperty(PropertyName = "ParkedHours")]
        public int ParkedHours { get; set; }

        [JsonProperty(PropertyName = "CarPlateNumber")]
        public string CarPlateNumber { get; set; }

        [JsonProperty(PropertyName = "TotaAmount")]
        public int TotaAmount { get; set; }

        [JsonProperty(PropertyName = "HasPenalty")]
        public bool HasPenalty { get; set; }
    }
}