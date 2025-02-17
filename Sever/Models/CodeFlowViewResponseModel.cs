using System.Text.Json.Serialization;

namespace Sever.Models
{
    public class CodeFlowViewResponseModel : CodeFlowResponseModel
    {
        [JsonPropertyName("redirect_uri")]
        public required string RedirectUri { get; set; }
    }
}
