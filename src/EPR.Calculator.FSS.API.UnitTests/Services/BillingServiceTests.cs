namespace EPR.Calculator.FSS.API.UnitTests
{
    using AutoFixture;
    using EPR.Calculator.FSS.API;
    using EPR.Calculator.FSS.API.Common;
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

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture();
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
            var instance = new BillingService(_storageServiceMock.Object);
            _storageServiceMock.Setup(x => x.GetFileContents(It.IsAny<string>())).Throws<FileNotFoundException>();

            Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => instance.GetBillingData(calcRunId: 1)).Wait();
        }

        [TestMethod]
        public void Expect_FileContents()
        {
            var instance = new BillingService(_storageServiceMock.Object);
            _storageServiceMock.Setup(x => x.GetFileContents(It.IsAny<string>())).ReturnsAsync("Some content");

            var result = instance.GetBillingData(calcRunId: 1);
            result.Wait();

            var content = result.Result;

            // Assert
            using (new AssertionScope())
            {
                content.Should().NotBeNullOrEmpty();
                content.Should().Be("Some content");
                _storageServiceMock.Verify(n => n.GetFileContents(It.IsAny<string>()), Times.Once);
            }
        }
    }
}