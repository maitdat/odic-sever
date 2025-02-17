using Sever.Entity;

namespace Sever.Models
{
    public class AuthCodeItem
    {
        public required AuthenticationRequestModel AuthenticationRequest { get; set; }
        public required User User { get; set; }
        public required string[] Scopes { get; set; }
    }
}
