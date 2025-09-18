namespace PatientConsultationAPI.DTOs
{
    public class CreatePatientDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }
}
