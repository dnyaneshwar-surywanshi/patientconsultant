using Microsoft.EntityFrameworkCore;
using PatientConsultationAPI.Data;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;

namespace PatientConsultationAPI.Repositories.Implementations
{
    public class ConsultationRepository : GenericRepository<Consultation>, IConsultationRepository
    {
        public ConsultationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId)
        {
            return await _context.Consultations
                                 .Include(c => c.Patient)
                                 .Where(c => c.PatientId == patientId)
                                 .ToListAsync();
        }

        public async Task<Consultation?> GetConsultationByIdAsync(int id)
        {
            return await _context.Consultations
                                 .Include(c => c.Patient)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
