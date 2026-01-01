namespace XYZUniversityPortal.Models
{
    public class Student
    {
        public string? StudentId { get; set; }

        public DateTime EnrollmentDate { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }

    }
}
