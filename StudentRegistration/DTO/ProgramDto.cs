namespace StudentRegistration.DTO
{
    public class ProgramDto
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public List<SubjectDto> Subjects { get; set; }
    }
    public class SubjectDto
    {
        public int Id { get; set; }
        public string SubjectNames { get; set; }
    }
}
