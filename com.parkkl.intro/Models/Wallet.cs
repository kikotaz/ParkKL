using System;
using com.parkkl.intro.Abstractions;
using Newtonsoft.Json;

namespace com.parkkl.intro.Models
{
    public class Wallet : TableData
    {
        [JsonProperty(PropertyName = "Email")]
        public String Email { get; set; }

        [JsonProperty(PropertyName = "Balance")]
        public String Balance { get; set; }
    }
}