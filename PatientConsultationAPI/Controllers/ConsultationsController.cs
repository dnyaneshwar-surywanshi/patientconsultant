using Microsoft.AspNetCore.Mvc;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Services;

namespace PatientConsultationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationsController : ControllerBase
    {
        private readonly ConsultationService _consultationService;

        public ConsultationsController(ConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetConsultationsByPatientId(int patientId)
        {
            var consultations = await _consultationService.GetConsultationsByPatientIdAsync(patientId);
            return Ok(consultations);
        }

        // make sure client sends multipart/form-data
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateConsultation([FromForm] CreateConsultationDto dto)
        {
            try
            {
                var consultation = await _consultationService.CreateConsultationAsync(dto);
                return CreatedAtAction(nameof(GetConsultationsByPatientId), new { patientId = dto.PatientId }, consultation);
            }
            catch (ArgumentException ex)
            {
                // return friendly validation message
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromForm] UpdateConsultationDto dto)
        {
            try
            {
                var updatedConsultation = await _consultationService.UpdateConsultationAsync(id, dto);
                if (updatedConsultation == null) return NotFound();
                return Ok(updatedConsultation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
