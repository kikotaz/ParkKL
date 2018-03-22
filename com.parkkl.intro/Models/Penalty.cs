using System;
using com.parkkl.intro.Abstractions;
using Newtonsoft.Json;

namespace com.parkkl.intro.Models
{
    class Penalty : TableData
    {
        [JsonProperty(PropertyName = "Email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "ExceededHours")]
        public string ExceededHours { get; set; }

        [JsonProperty(PropertyName = "TotaAmount")]
        public int TotaAmount { get; set; }

        [JsonProperty(PropertyName = "PenaltyPaid")]
        public bool PenaltyPaid { get; set; }

        [JsonProperty(PropertyName = "ParkingId")]
        public string ParkingId{ get; set; }

    }
}