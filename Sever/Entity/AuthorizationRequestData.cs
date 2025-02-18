using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sever.Entity
{
    public class AuthorizationRequestData
    {
        public int Id { get; set; }
        public string AuthorizationCode { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        [Column("UserId")]
        public required User User { get; set; } 
        public string Nonce { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
