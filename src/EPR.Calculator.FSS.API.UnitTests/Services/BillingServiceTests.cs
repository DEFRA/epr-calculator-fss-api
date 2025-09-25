namespace EPR.Calculator.FSS.API.UnitTests
{
    using API;
    using AutoFixture;
    using Common;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.IO;

    [TestClass]
    public class BillingServiceTests
    {
        private BillingService _testClass;
        private Mock<IBlobStorageService> _storageServiceMock;
        private IFixture _fixture = null!;

        [TestInitialize]
        public void SetUp()
        {
             _fixture = new Fixture();
             _storageServiceMock = new Mock<IBlobStorageService>();
             _testClass = new BillingService(_storageServiceMock.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new BillingService(_storageServiceMock.Object);

            // Assert
            instance.Should().NotBeNull();
        }

        [TestMethod]
        public void Expect_FileNotFoundException_WhenJsonFile_NotExists()
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var fileName = $"{runId}billing.json";

            var instance = new BillingService(_storageServiceMock.Object);
            _storageServiceMock.Setup(x => x.GetFileContents(fileName)).Throws<FileNotFoundException>();

            Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => instance.GetBillingData(runId)).Wait();
        }

        [TestMethod]
        public void Expect_FileContents()
        {
            // Arrange
            var runId = _fixture.Create<int>();
            var fileName = $"{runId}billing.json";

            var instance = new BillingService(_storageServiceMock.Object);
            _storageServiceMock.Setup(x => x.GetFileContents(fileName)).ReturnsAsync("Some content");

            // Act
            var result = instance.GetBillingData(runId);
            result.Wait();

            var content = result.Result;

            // Assert
            using (new AssertionScope())
            {
                content.Should().NotBeNullOrEmpty();
                content.Should().Be("Some content");
                _storageServiceMock.Verify(n => n.GetFileContents(fileName), Times.Once);
            }
        }
    }
}