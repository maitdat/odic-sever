﻿using System.Text.Json.Serialization;

namespace Sever.Models
{
    public class CodeFlowResponseModel
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("state")]
        public required string State { get; set; }
    }
}
