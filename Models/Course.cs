using System.ComponentModel.DataAnnotations;

namespace XYZUniversityPortal.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        public string? Title { get; set; }

        [Range(1, 5)]
        public int Credits { get; set; }
        [Required]

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Instructor? Instructor { get; set; }
        public string? InstructorId { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }

    }
}
