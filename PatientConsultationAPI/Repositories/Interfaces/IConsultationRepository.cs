using PatientConsultationAPI.Models;

namespace PatientConsultationAPI.Repositories.Interfaces
{
    public interface IConsultationRepository : IRepository<Consultation>
    {
        Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId);
        Task<Consultation> GetConsultationByIdAsync(int id);
    }
}
