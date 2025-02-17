using System.Text.Json.Serialization;

namespace Sever.Models
{
    public class AuthTokenResponseModel : RefreshResponseModel
    {
        [JsonPropertyName("id_token")]
        public required string IdToken { get; set; }
    }
}
