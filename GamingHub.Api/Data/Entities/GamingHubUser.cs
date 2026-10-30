using Microsoft.AspNetCore.Identity;

namespace GamingHub.Api.Data.Entities
{
    public class GamingHubUser : IdentityUser
    {
        public bool ForceRelogin { get; set; }
    }
}
