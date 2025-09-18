namespace PatientConsultationAPI.Models
{
    public class Consultation
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; } = string.Empty;

        public string AttachmentFileName { get; set; } = string.Empty;

        public ConsultationStatus Status { get; set; } = ConsultationStatus.Pending;

        public Patient Patient { get; set; }
    }
}
