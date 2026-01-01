namespace XYZUniversityPortal.Models
{
    public class Instructor
    {
        // PK and FK to AspNetUsers
        public string? InstructorId { get; set; }

        public int DepartmentId { get; set; }

        public string? Office { get; set; }
        public Department? Department { get; set; }
        public ICollection<Course>? Courses { get; set; }

    }
}
