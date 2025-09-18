namespace PatientConsultationAPI.Models
{
    public enum ConsultationStatus
    {
        Pending,    // Consultation is scheduled but not yet done
        Completed,  // Consultation has been completed
        Canceled    // Consultation has been canceled
    }
}
