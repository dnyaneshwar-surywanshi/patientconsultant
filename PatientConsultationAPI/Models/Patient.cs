namespace PatientConsultationAPI.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
