using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;

namespace PatientConsultationAPI.Services
{
    public class PatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync() => await _patientRepository.GetAllAsync();

        public async Task<Patient?> GetPatientDetailsAsync(int id)
        {
            return await _patientRepository.GetPatientWithConsultationsAsync(id);
        }

        public async Task<Patient> AddPatientAsync(CreatePatientDto dto)
        {
            var patient = new Patient
            {
                Name = dto.Name,
                Age = dto.Age,
                Gender = dto.Gender
            };

            await _patientRepository.AddAsync(patient);
            await _patientRepository.SaveAsync();

            return patient;
        }

        public async Task<Patient?> UpdatePatientAsync(int id, CreatePatientDto dto)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null) return null;

            patient.Name = dto.Name;
            patient.Age = dto.Age;
            patient.Gender = dto.Gender;

            _patientRepository.Update(patient);
            await _patientRepository.SaveAsync();

            return patient;
        }
    }
}
