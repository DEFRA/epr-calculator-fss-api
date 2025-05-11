namespace EPR.Calculator.FSS.API.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.FSS.API;
    using EPR.Calculator.FSS.API.Common;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Moq;

    [TestClass]
    public class BillingServiceTests
    {
        private BillingService _testClass;
        private Mock<IBlobStorageService> _storageService;
        private ApplicationDBContext _context;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture();
            _storageService = new Mock<IBlobStorageService>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new ApplicationDBContext(dbContextOptions);
            _context.Database.EnsureCreated();
            _testClass = new BillingService(_storageService.Object, _context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new BillingService(_storageService.Object, _context);

            // Assert
            instance.Should().NotBeNull();
        }

        [TestMethod]
        public void Expect_KeyNotFoundException()
        {
            var instance = new BillingService(_storageService.Object, _context);

            Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                () => instance.GetBillingData(calcRunId: 1)).Wait();
        }
    }
}