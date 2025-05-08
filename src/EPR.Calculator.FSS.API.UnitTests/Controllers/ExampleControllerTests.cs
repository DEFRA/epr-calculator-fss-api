using AutoFixture;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.FSS.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Controllers
{
    [TestClass]
    public class ExampleControllerTests
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly ExampleController _controller;

        public ExampleControllerTests()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _controller = new ExampleController(_mockContext.Object);
        }

        [TestMethod]
        public async Task Get_ReturnsOkResult_WithFinancialYears()
        {
            // Arrange
            var financialYears = new List<CalculatorRunFinancialYear>
            {
                new CalculatorRunFinancialYear { Name = "2024-25" },
                new CalculatorRunFinancialYear { Name = "2025-26" }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<CalculatorRunFinancialYear>>();
            mockDbSet.As<IQueryable<CalculatorRunFinancialYear>>().Setup(m => m.Provider).Returns(financialYears.Provider);
            mockDbSet.As<IQueryable<CalculatorRunFinancialYear>>().Setup(m => m.Expression).Returns(financialYears.Expression);
            mockDbSet.As<IQueryable<CalculatorRunFinancialYear>>().Setup(m => m.ElementType).Returns(financialYears.ElementType);
            mockDbSet.As<IQueryable<CalculatorRunFinancialYear>>().Setup(m => m.GetEnumerator()).Returns(financialYears.GetEnumerator());

            _mockContext.Setup(c => c.FinancialYears).Returns(mockDbSet.Object);

            // Act
            var result = _controller.Get() as OkObjectResult;

            // Assert

            Assert.AreEqual(200, result.StatusCode);
            var value = result.Value as IEnumerable<CalculatorRunFinancialYear>;
            value.Should().BeEquivalentTo(financialYears);
        }
    }
}
