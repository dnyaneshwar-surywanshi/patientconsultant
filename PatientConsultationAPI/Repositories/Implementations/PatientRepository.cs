using Microsoft.EntityFrameworkCore;
using PatientConsultationAPI.Data;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;

namespace PatientConsultationAPI.Repositories.Implementations
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(AppDbContext context) : base(context) { }

        public async Task<Patient?> GetPatientWithConsultationsAsync(int patientId)
        {
            return await _context.Patients.Include(p => p.Consultations).FirstOrDefaultAsync(p => p.Id == patientId);
        }
    }
}
