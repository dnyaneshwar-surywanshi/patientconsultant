using PatientConsultationAPI.Models;

namespace PatientConsultationAPI.DTOs
{
    public class UpdateConsultationDto
    {
        public DateTime? Date { get; set; }
        public string? Notes { get; set; }
        public IFormFile? Attachment { get; set; }
        public ConsultationStatus Status { get; set; }
    }
}
