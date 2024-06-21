using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FileProcessing.WebAPI.Models
{
    public class XMLUser
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("rate")]
        public int Rate { get; set; }
    }
}
