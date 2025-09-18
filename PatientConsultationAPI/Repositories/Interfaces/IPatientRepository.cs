using PatientConsultationAPI.Models;

namespace PatientConsultationAPI.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient?> GetPatientWithConsultationsAsync(int patientId);
    }
}
