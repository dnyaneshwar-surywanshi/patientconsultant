using Moq;
using Microsoft.AspNetCore.Mvc;
using PatientConsultationAPI.Controllers;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;
using PatientConsultationAPI.Services;
using FluentAssertions;

namespace PatientConsultationAPI.Tests.Controllers
{
    public class PatientsControllerTests
    {
        private readonly Mock<IPatientRepository> _repoMock;
        private readonly PatientService _service;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            _repoMock = new Mock<IPatientRepository>();
            _service = new PatientService(_repoMock.Object);
            _controller = new PatientsController(_service);
        }

        [Fact]
        public async Task GetAllPatients_ShouldReturnOk()
        {
            var patients = new List<Patient> { new Patient { Id = 1, Name = "John" } };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(patients);

            var result = await _controller.GetAllPatients() as OkObjectResult;

            result.Value.Should().BeEquivalentTo(patients);
        }

        [Fact]
        public async Task GetAllPatients_ShouldReturnEmptyList_WhenNoPatients()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Patient>());

            var result = await _controller.GetAllPatients() as OkObjectResult;

            ((IEnumerable<Patient>)result.Value).Should().BeEmpty();
        }

        [Fact]
        public async Task CreatePatient_ShouldReturnCreatedAtAction()
        {
            var dto = new CreatePatientDto { Name = "Jane", Age = 30, Gender = "F" };
            var patient = new Patient { Id = 1, Name = "Jane" };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Patient>())).Returns(Task.CompletedTask).Callback<Patient>(p => patient = p);
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _controller.CreatePatient(dto) as CreatedAtActionResult;

            result.Value.Should().BeEquivalentTo(patient);
        }

        [Fact]
        public async Task GetPatientDetails_ShouldReturnOk_WhenFound()
        {
            var patient = new Patient { Id = 1, Name = "John" };
            _repoMock.Setup(r => r.GetPatientWithConsultationsAsync(1)).ReturnsAsync(patient);

            var result = await _controller.GetPatientDetails(1) as OkObjectResult;

            result.Value.Should().BeEquivalentTo(patient);
        }

        [Fact]
        public async Task GetPatientDetails_ShouldReturnNotFound_WhenNull()
        {
            _repoMock.Setup(r => r.GetPatientWithConsultationsAsync(1)).ReturnsAsync((Patient)null);

            var result = await _controller.GetPatientDetails(1);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdatePatient_ShouldReturnOk_WhenUpdated()
        {
            var patient = new Patient { Id = 1, Name = "Updated", Age = 40, Gender = "M" };
            var dto = new CreatePatientDto { Name = "Updated", Age = 40, Gender = "M" };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);
            _repoMock.Setup(r => r.Update(It.IsAny<Patient>()));
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _controller.UpdatePatient(1, dto) as OkObjectResult;

            result.Value.Should().BeEquivalentTo(patient);
        }

        [Fact]
        public async Task UpdatePatient_ShouldReturnNotFound_WhenPatientDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient)null);

            var result = await _controller.UpdatePatient(999, new CreatePatientDto());

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
