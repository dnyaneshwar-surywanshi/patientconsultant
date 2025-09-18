using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PatientConsultationAPI.Controllers;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;
using PatientConsultationAPI.Services;

namespace PatientConsultationAPI.Tests.Controllers
{
    public class ConsultationsControllerTests
    {
        private readonly Mock<IConsultationRepository> _repoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly ConsultationService _service;
        private readonly ConsultationsController _controller;

        public ConsultationsControllerTests()
        {
            _repoMock = new Mock<IConsultationRepository>();
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _service = new ConsultationService(_repoMock.Object, _envMock.Object);
            _controller = new ConsultationsController(_service);
        }

        [Fact]
        public async Task GetConsultationsByPatientId_ReturnsOk_WithConsultations()
        {
            // Arrange
            int patientId = 1;
            var consultations = new List<Consultation>
            {
                new Consultation { Id = 1, PatientId = patientId, Notes = "General check-up", Status = ConsultationStatus.Pending }
            };

            _repoMock.Setup(r => r.GetConsultationsByPatientIdAsync(patientId))
                     .ReturnsAsync(consultations);

            // Act
            var result = await _controller.GetConsultationsByPatientId(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Consultation>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task CreateConsultation_ReturnsCreatedAtAction_WhenValid()
        {
            // Arrange
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("Dummy PDF content");
            writer.Flush();
            ms.Position = 0;

            var formFile = new FormFile(ms, 0, ms.Length, "file", "test.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var dto = new CreateConsultationDto
            {
                PatientId = 1,
                Date = DateTime.UtcNow,
                Notes = "First consultation",
                Attachment = formFile
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Consultation>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateConsultation(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var consultation = Assert.IsType<Consultation>(createdResult.Value);
            Assert.Equal(dto.PatientId, consultation.PatientId);
            Assert.Equal(ConsultationStatus.Pending, consultation.Status);
        }

        [Fact]
        public async Task CreateConsultation_ReturnsBadRequest_WhenAttachmentMissing()
        {
            // Arrange
            var dto = new CreateConsultationDto
            {
                PatientId = 1,
                Date = DateTime.UtcNow,
                Notes = "Missing file"
            };

            // Act
            var result = await _controller.CreateConsultation(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Attachment is required", badRequest.Value.ToString());
        }

        [Fact]
        public async Task UpdateConsultation_ReturnsOk_WhenUpdated()
        {
            // Arrange
            int consultationId = 1;
            var existingConsultation = new Consultation
            {
                Id = consultationId,
                PatientId = 1,
                Date = DateTime.UtcNow,
                Notes = "Old Notes",
                Status = ConsultationStatus.Pending
            };

            var updateDto = new UpdateConsultationDto
            {
                Date = DateTime.UtcNow.AddDays(1),
                Notes = "Updated Notes",
                Status = ConsultationStatus.Completed
            };

            _repoMock.Setup(r => r.GetConsultationByIdAsync(consultationId))
                     .ReturnsAsync(existingConsultation);

            _repoMock.Setup(r => r.Update(It.IsAny<Consultation>()));
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateConsultation(consultationId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedConsultation = Assert.IsType<Consultation>(okResult.Value);
            Assert.Equal(updateDto.Notes, updatedConsultation.Notes);
            Assert.Equal(updateDto.Status, updatedConsultation.Status);
        }

        [Fact]
        public async Task UpdateConsultation_ReturnsNotFound_WhenConsultationDoesNotExist()
        {
            // Arrange
            int consultationId = 999;
            var updateDto = new UpdateConsultationDto
            {
                Date = DateTime.UtcNow,
                Notes = "Nothing",
                Status = ConsultationStatus.Pending
            };

            _repoMock.Setup(r => r.GetConsultationByIdAsync(consultationId))
                     .ReturnsAsync((Consultation)null);

            // Act
            var result = await _controller.UpdateConsultation(consultationId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
