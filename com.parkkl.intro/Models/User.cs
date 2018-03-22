using com.parkkl.intro.Abstractions;
using Newtonsoft.Json;

namespace com.parkkl.intro.Models
{
    public class User : TableData
    {
        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "EmailConfirmed")]
        public bool EmailConfirmed { get; set; }

        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "WalletBalance")]
        public double WalletBalance { get; set; }
        
    }
}