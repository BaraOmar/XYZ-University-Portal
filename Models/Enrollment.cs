namespace XYZUniversityPortal.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public string? StudentId { get; set; }
        public int CourseId { get; set; }
        public double? Grade { get; set; }
        public Student? Student { get; set; }
        public Course? Course { get; set; }

    }
}
