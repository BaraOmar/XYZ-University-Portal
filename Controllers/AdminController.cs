using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XYZUniversityPortal.Data;
using XYZUniversityPortal.Models;
using XYZUniversityPortal.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace XYZUniversityPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;


        public AdminController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var roles = new[] { "Admin", "Instructor", "Student" };

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                bool hasDepartment = _context.Instructors
                    .Any(i => i.InstructorId == user.Id && i.DepartmentId != 0);

                model.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = userRoles,
                    SelectedRole = userRoles.FirstOrDefault(),
                    AllRoles = roles,
                    HasDepartment = hasDepartment   // ✅ HERE
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
            if (string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Please select a role.";
                return RedirectToAction(nameof(Users));
            }


            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> AssignInstructor(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();


            var instructor = await _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.InstructorId == userId);

            ViewData["Departments"] = new SelectList(
                _context.Departments,
                "DepartmentId",
                "Name"
            );

            ViewData["UserId"] = user.Id;
            ViewData["Email"] = user.Email;

            // 👇 THIS IS THE IMPORTANT PART
            ViewData["CurrentDepartment"] = instructor?.Department?.Name;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructor(string userId, int departmentId)
        {
            if (departmentId == 0)
            {
                TempData["Error"] = "Please select a department.";
                return RedirectToAction(nameof(AssignInstructor), new { userId });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.InstructorId == userId);

            if (instructor != null)
            {
                // ❌ DO NOT allow changing department
                TempData["Error"] = "Instructor department cannot be changed once assigned.";
                return RedirectToAction(nameof(AssignInstructor), new { userId });
            }



            // ✅ First-time assignment only
            instructor = new Instructor
            {
                InstructorId = userId,
                DepartmentId = departmentId
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }


        public async Task<IActionResult> AssignCourses(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
                return NotFound();

            var instructor = await _context.Instructors
                .Include(i => i.Department)
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.InstructorId == instructorId);

            if (instructor == null)
            {
                TempData["Error"] = "You must assign a department to the instructor before assigning courses.";
                return RedirectToAction(nameof(Users));
            }

            var availableCourses = _context.Courses
                .Where(c =>
                    c.InstructorId == null &&                    // one-to-many
                    c.DepartmentId == instructor.DepartmentId   // same department
                )
                .ToList();

            ViewData["InstructorId"] = instructorId;
            ViewData["InstructorCourses"] = instructor.Courses;
            ViewData["InstructorDepartment"] = instructor.Department?.Name;
            ViewData["Courses"] = new SelectList(
                availableCourses,
                "CourseId",
                "Title"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCourses(string instructorId, int courseId)
        {
            if (string.IsNullOrEmpty(instructorId))
                return BadRequest();

            if (courseId == 0)
            {
                TempData["Error"] = "Please select a course to assign.";
                return RedirectToAction(nameof(AssignCourses), new { instructorId });
            }

            var instructor = await _context.Instructors
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.InstructorId == instructorId);

            if (instructor == null)
                return NotFound();

            if (instructor.Courses.Count >= 3)
            {
                TempData["Error"] = "Instructor cannot have more than 3 courses.";
                return RedirectToAction(nameof(AssignCourses), new { instructorId });
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            course.InstructorId = instructorId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AssignCourses), new { instructorId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourse(string instructorId, int courseId)
        {
            if (string.IsNullOrEmpty(instructorId) || courseId == 0)
                return BadRequest();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            // safety check
            if (course.InstructorId != instructorId)
                return BadRequest();

            // ✅ un-assign
            course.InstructorId = null;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AssignCourses), new { instructorId });
        }



    }
}
