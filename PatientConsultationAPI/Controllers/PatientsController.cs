using Microsoft.AspNetCore.Mvc;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Services;

namespace PatientConsultationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            var patient = await _patientService.AddPatientAsync(dto);
            return CreatedAtAction(nameof(GetAllPatients), new { id = patient.Id }, patient);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientDetails(int id)
        {
            var patient = await _patientService.GetPatientDetailsAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] CreatePatientDto dto)
        {
            var updatedPatient = await _patientService.UpdatePatientAsync(id, dto);
            if (updatedPatient == null) return NotFound();

            return Ok(updatedPatient);
        }
    }
}