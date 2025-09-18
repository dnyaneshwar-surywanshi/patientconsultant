using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;

namespace PatientConsultationAPI.Services
{
    public class ConsultationService
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly IWebHostEnvironment _environment;

        public ConsultationService(IConsultationRepository consultationRepository, IWebHostEnvironment environment)
        {
            _consultationRepository = consultationRepository;
            _environment = environment;
        }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId)
        {
            return await _consultationRepository.GetConsultationsByPatientIdAsync(patientId);
        }

        public async Task<Consultation> CreateConsultationAsync(CreateConsultationDto dto)
        {
            if (dto.Attachment == null || dto.Attachment.Length == 0)
                throw new ArgumentException("Attachment is required.");

            // allowed extensions and MIME types
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "application/pdf",
                                      "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

            var rawFileName = Path.GetFileName(dto.Attachment.FileName ?? string.Empty); // defensive
            var extension = Path.GetExtension(rawFileName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("Uploaded file has no extension. Please include a valid file extension.");

            // sometimes browsers or client libs send content-type differently; check both
            var contentType = dto.Attachment.ContentType?.ToLowerInvariant() ?? string.Empty;

            if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Any(ct => contentType.Contains(ct)))
            {
                throw new ArgumentException($"Allowed file types: {string.Join(", ", allowedExtensions)} (mime: {string.Join(", ", allowedContentTypes)})");
            }

            // Save file with sanitized name
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var safeFileName = $"{timestamp}_{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Attachment.CopyToAsync(stream);
            }

            var consultation = new Consultation
            {
                PatientId = dto.PatientId,
                Date = dto.Date,
                Notes = dto.Notes,
                AttachmentFileName = safeFileName,
                Status = ConsultationStatus.Pending
            };

            await _consultationRepository.AddAsync(consultation);
            await _consultationRepository.SaveAsync();

            return consultation;
        }

        public async Task<Consultation?> UpdateConsultationAsync(int id, UpdateConsultationDto dto)
        {
            var consultation = await _consultationRepository.GetConsultationByIdAsync(id);
            if (consultation == null) return null;

            consultation.Date = dto.Date ?? consultation.Date;
            consultation.Notes = dto.Notes ?? consultation.Notes;
            consultation.Status = dto.Status;

            // Handle file replacement
            if (dto.Attachment != null)
            {
                var fileName = $"{Guid.NewGuid()}_{dto.Attachment.FileName}";
                var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }
                consultation.AttachmentFileName = fileName;
            }

            _consultationRepository.Update(consultation);
            await _consultationRepository.SaveAsync();

            return consultation;
        }
    }
}
