using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;
using PatientConsultationAPI.Services;
using System.Text;

namespace PatientConsultationAPI.Tests.Services
{
    public class ConsultationServiceTests
    {
        private readonly Mock<IConsultationRepository> _repositoryMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly ConsultationService _service;
        private readonly string _fakeUploadPath;

        public ConsultationServiceTests()
        {
            _repositoryMock = new Mock<IConsultationRepository>();
            _envMock = new Mock<IWebHostEnvironment>();
            _fakeUploadPath = Path.Combine(Path.GetTempPath(), "TestUploads");
            if (!Directory.Exists(_fakeUploadPath))
                Directory.CreateDirectory(_fakeUploadPath);

            _envMock.Setup(e => e.ContentRootPath).Returns(_fakeUploadPath);

            _service = new ConsultationService(_repositoryMock.Object, _envMock.Object);
        }

        [Fact]
        public async Task GetConsultationsByPatientIdAsync_ReturnsConsultations()
        {
            // Arrange
            var patientId = 1;
            var consultations = new List<Consultation>
            {
                new Consultation { Id = 1, PatientId = patientId, Notes = "Checkup" }
            };

            _repositoryMock
                .Setup(r => r.GetConsultationsByPatientIdAsync(patientId))
                .ReturnsAsync(consultations);

            // Act
            var result = await _service.GetConsultationsByPatientIdAsync(patientId);

            // Assert
            Assert.Single(result);
            Assert.Equal(patientId, ((List<Consultation>)result)[0].PatientId);
            _repositoryMock.Verify(r => r.GetConsultationsByPatientIdAsync(patientId), Times.Once);
        }

        [Fact]
        public async Task CreateConsultationAsync_SuccessfullyCreatesConsultation()
        {
            // Arrange
            var patientId = 1;
            var fileName = "report.pdf";

            var fakeFile = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("Dummy file content")),
                0,
                20,
                "Data",
                fileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var dto = new CreateConsultationDto
            {
                PatientId = patientId,
                Date = DateTime.UtcNow,
                Notes = "Blood test report",
                Attachment = fakeFile
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Consultation>()))
                           .Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateConsultationAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patientId, result.PatientId);
            Assert.Equal("Blood test report", result.Notes);
            Assert.NotNull(result.AttachmentFileName);
            Assert.EndsWith(".pdf", result.AttachmentFileName);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateConsultationAsync_ThrowsException_WhenFileIsMissing()
        {
            // Arrange
            var dto = new CreateConsultationDto
            {
                PatientId = 1,
                Date = DateTime.UtcNow,
                Notes = "Missing file",
                Attachment = null
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateConsultationAsync(dto));

            Assert.Equal("Attachment is required.", ex.Message);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
        }

        [Fact]
        public async Task CreateConsultationAsync_ThrowsException_WhenInvalidFileType()
        {
            // Arrange
            var fakeFile = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("Invalid content")),
                0,
                20,
                "Data",
                "report.exe"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            var dto = new CreateConsultationDto
            {
                PatientId = 1,
                Date = DateTime.UtcNow,
                Notes = "Invalid file type",
                Attachment = fakeFile
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateConsultationAsync(dto));

            Assert.Contains("Allowed file types", ex.Message);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
        }

        [Fact]
        public async Task UpdateConsultationAsync_ReturnsUpdatedConsultation_WhenFound()
        {
            // Arrange
            var consultationId = 10;
            var existingConsultation = new Consultation
            {
                Id = consultationId,
                PatientId = 1,
                Notes = "Old Notes",
                Date = DateTime.UtcNow.AddDays(-1),
                Status = ConsultationStatus.Pending
            };

            var updateDto = new UpdateConsultationDto
            {
                Date = DateTime.UtcNow,
                Notes = "Updated Notes",
                Status = ConsultationStatus.Completed,
                Attachment = null
            };

            _repositoryMock.Setup(r => r.GetConsultationByIdAsync(consultationId))
                           .ReturnsAsync(existingConsultation);

            _repositoryMock.Setup(r => r.Update(It.IsAny<Consultation>()));
            _repositoryMock.Setup(r => r.SaveAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateConsultationAsync(consultationId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Notes", result.Notes);
            Assert.Equal(ConsultationStatus.Completed, result.Status);
            _repositoryMock.Verify(r => r.Update(existingConsultation), Times.Once);
            _repositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateConsultationAsync_ReturnsNull_WhenConsultationNotFound()
        {
            // Arrange
            var consultationId = 999;
            var updateDto = new UpdateConsultationDto
            {
                Notes = "Attempt update on missing consultation"
            };

            _repositoryMock.Setup(r => r.GetConsultationByIdAsync(consultationId))
                           .ReturnsAsync((Consultation)null);

            // Act
            var result = await _service.UpdateConsultationAsync(consultationId, updateDto);

            // Assert
            Assert.Null(result);
            _repositoryMock.Verify(r => r.Update(It.IsAny<Consultation>()), Times.Never);
        }

        [Fact]
        public async Task UpdateConsultationAsync_UpdatesAttachment_WhenFileProvided()
        {
            // Arrange
            var consultationId = 20;
            var existingConsultation = new Consultation
            {
                Id = consultationId,
                PatientId = 1,
                Notes = "Initial Notes",
                Date = DateTime.UtcNow,
                Status = ConsultationStatus.Pending
            };

            var fakeFile = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("New attachment content")),
                0,
                20,
                "Data",
                "updated_report.pdf"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var updateDto = new UpdateConsultationDto
            {
                Date = DateTime.UtcNow,
                Notes = "Updated with new file",
                Status = ConsultationStatus.Completed,
                Attachment = fakeFile
            };

            _repositoryMock.Setup(r => r.GetConsultationByIdAsync(consultationId))
                           .ReturnsAsync(existingConsultation);
            _repositoryMock.Setup(r => r.Update(It.IsAny<Consultation>()));
            _repositoryMock.Setup(r => r.SaveAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateConsultationAsync(consultationId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated with new file", result.Notes);
            Assert.Equal(ConsultationStatus.Completed, result.Status);
            Assert.NotNull(result.AttachmentFileName);
            Assert.Contains(".pdf", result.AttachmentFileName);

            _repositoryMock.Verify(r => r.Update(existingConsultation), Times.Once);
            _repositoryMock.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
