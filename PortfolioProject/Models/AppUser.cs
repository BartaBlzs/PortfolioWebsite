using Microsoft.AspNetCore.Identity;

namespace Auth.Models
{
    public class AppUser : IdentityUser
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
