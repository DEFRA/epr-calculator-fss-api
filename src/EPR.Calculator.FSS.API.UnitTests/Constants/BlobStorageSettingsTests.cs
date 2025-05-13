using AutoFixture;
using EPR.Calculator.FSS.API.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.FSS.API.UnitTests.Constants
{
    [TestClass]
    public class BlobStorageSettingsTests
    {
        public BlobStorageSettingsTests()
        {
            Fixture = new Fixture();
            this.TestClass = new BlobStorageSettings();
        }

        private BlobStorageSettings TestClass { get; init; }

        private IFixture Fixture { get; init; }

        [TestMethod]
        public void CanSetAndGetConnectionString()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ConnectionString = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ConnectionString);
        }

        [TestMethod]
        public void CanSetAndGetContainerName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ContainerName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ContainerName);
        }

        [TestMethod]
        public void CanSetAndGetCsvFileName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.CsvFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CsvFileName);
        }
    }
}