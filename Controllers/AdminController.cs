using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using XYZUniversityPortal.Models.ViewModels;

namespace XYZUniversityPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var roles = new[] { "Admin", "Instructor", "Student" };

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                model.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = userRoles,
                    SelectedRole = userRoles.FirstOrDefault(), // ← IMPORTANT
                    AllRoles = roles
                });

            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove existing roles (single-role system)
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Users));
        }


    }
}
