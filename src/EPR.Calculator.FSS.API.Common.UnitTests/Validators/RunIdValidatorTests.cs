using AutoFixture;
using EPR.Calculator.FSS.API.Common.Validators;

namespace EPR.Calculator.FSS.API.Common.UnitTests.Validators
{
    /// <summary>
    /// Unit tests for the <see cref="RunIdValidator"/> class.
    /// </summary>
    [TestClass]
    public class RunIdValidatorTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunIdValidatorTests"/> class.
        /// </summary>
        public RunIdValidatorTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = new RunIdValidator();
        }

        private IFixture Fixture { get; init; }

        private RunIdValidator TestClass { get; init; }

        /// <summary>
        /// Test the construction of the <see cref="RunIdValidator"/> class.
        /// </summary>
        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new RunIdValidator();

            // Assert
            Assert.IsNotNull(instance);
        }

        /// <summary>
        /// Test the validation of the run ID.
        /// </summary>
        /// <param name="runId">A run ID to test.</param>
        /// <param name="isValid">Whether the given run ID is valid.</param>
        [TestMethod]
        [DataRow(0, false)]
        public void Validate(int runId, bool isValid)
        {
            // Arrange
            var instance = new RunIdValidator();

            // Act
            var result = instance.Validate(runId);

            // Assert
            Assert.AreEqual(isValid, result.IsValid);
        }
    }
}