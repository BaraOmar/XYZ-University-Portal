namespace XYZUniversityPortal.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string? Name { get; set; }
        public ICollection<Course>? Courses { get; set; }
        public ICollection<Instructor>? Instructors { get; set; }


    }
}
