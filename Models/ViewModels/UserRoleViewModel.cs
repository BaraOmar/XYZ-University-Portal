using Microsoft.AspNetCore.Identity;

namespace XYZUniversityPortal.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public IdentityUser? User { get; set; }
        public IList<string>? Roles { get; set; }
        public string? SelectedRole { get; set; }
        public IEnumerable<string>? AllRoles { get; set; }
    }
}
