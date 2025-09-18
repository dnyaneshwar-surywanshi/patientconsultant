using Moq;
using PatientConsultationAPI.DTOs;
using PatientConsultationAPI.Models;
using PatientConsultationAPI.Repositories.Interfaces;
using PatientConsultationAPI.Services;
using FluentAssertions;

namespace PatientConsultationAPI.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _repoMock;
        private readonly PatientService _service;

        public PatientServiceTests()
        {
            _repoMock = new Mock<IPatientRepository>();
            _service = new PatientService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllPatientsAsync_ShouldReturnPatients()
        {
            var patients = new List<Patient> { new Patient { Id = 1, Name = "John" } };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(patients);

            var result = await _service.GetAllPatientsAsync();

            result.Should().BeEquivalentTo(patients);
        }

        [Fact]
        public async Task GetAllPatientsAsync_ShouldReturnEmpty_WhenNoPatients()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Patient>());

            var result = await _service.GetAllPatientsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddPatientAsync_ShouldAddAndReturnPatient()
        {
            var dto = new CreatePatientDto { Name = "Jane", Age = 30, Gender = "F" };
            Patient savedPatient = null;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Patient>())).Callback<Patient>(p => savedPatient = p).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddPatientAsync(dto);

            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            savedPatient.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task UpdatePatientAsync_ShouldReturnUpdatedPatient()
        {
            var patient = new Patient { Id = 1, Name = "Old", Age = 20, Gender = "M" };
            var dto = new CreatePatientDto { Name = "New", Age = 25, Gender = "F" };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);
            _repoMock.Setup(r => r.Update(It.IsAny<Patient>()));
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _service.UpdatePatientAsync(1, dto);

            result.Name.Should().Be("New");
            result.Age.Should().Be(25);
            result.Gender.Should().Be("F");
        }

        [Fact]
        public async Task UpdatePatientAsync_ShouldReturnNullIfNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Patient)null);

            var result = await _service.UpdatePatientAsync(1, new CreatePatientDto());

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdatePatientAsync_ShouldKeepValues_WhenDtoHasNullOrDefault()
        {
            var patient = new Patient { Id = 1, Name = "Old", Age = 20, Gender = "M" };
            var dto = new CreatePatientDto { Name = null, Age = 0, Gender = null };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(patient);
            _repoMock.Setup(r => r.Update(It.IsAny<Patient>()));
            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _service.UpdatePatientAsync(1, dto);

            result.Name.Should().BeNull();
            result.Age.Should().Be(0);
            result.Gender.Should().BeNull();
        }
    }
}
