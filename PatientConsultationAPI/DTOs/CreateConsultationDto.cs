using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PatientConsultationAPI.DTOs
{
    public class CreateConsultationDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Notes { get; set; } = string.Empty;

        [Required]
        public IFormFile Attachment { get; set; } = null!;
    }
}
