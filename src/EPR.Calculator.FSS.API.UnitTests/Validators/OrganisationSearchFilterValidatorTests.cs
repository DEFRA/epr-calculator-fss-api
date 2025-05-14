using EPR.Calculator.FSS.API.Common.Models;
using EPR.Calculator.FSS.API.Common.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.FSS.API.UnitTests.Validators;

[TestClass]
public class OrganisationSearchFilterValidatorTests
{
    private OrganisationSearchFilterValidator _organisationSearchFilterValidatorTest;

    [TestInitialize]
    public void Initialize()
    {
        _organisationSearchFilterValidatorTest = new OrganisationSearchFilterValidator();
    }

    [TestMethod]
    [DataRow("2025-05-10")]
    [DataRow("2025-01-01")]
    public void OrganisationSearchFilterValidator_Valid_Date_Parameter_Value(string createdOrModifiedAfter)
    {
        // Arrange
        OrganisationSearchFilter? orgSearch = new OrganisationSearchFilter();
        orgSearch = new OrganisationSearchFilter() { CreatedOrModifiedAfter = createdOrModifiedAfter };

        // Act
        var result = _organisationSearchFilterValidatorTest?.TestValidate<OrganisationSearchFilter>(orgSearch);

        // Assert
        result.Should().NotBeNull();
        result.ShouldNotHaveValidationErrorFor(x => x.CreatedOrModifiedAfter);
    }

    [TestMethod]
    [DataRow("25-05-10")]
    [DataRow("25-01-01")]
    public void OrganisationSearchFilterValidator_Invalid_Date_Parameter_Value(string createdOrModifiedAfter)
    {
        // Arrange
        OrganisationSearchFilter? orgSearch = new OrganisationSearchFilter();
        orgSearch = new OrganisationSearchFilter() { CreatedOrModifiedAfter = createdOrModifiedAfter };

        // Act
        var result = _organisationSearchFilterValidatorTest?.TestValidate<OrganisationSearchFilter>(orgSearch);

        // Assert
        result.Should().NotBeNull();
        result.ShouldHaveValidationErrorFor(x => x.CreatedOrModifiedAfter);
    }
}