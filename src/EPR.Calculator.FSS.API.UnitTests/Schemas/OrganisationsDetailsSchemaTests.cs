using System.Runtime.CompilerServices;
using System.Text.Json;
using EPR.Calculator.FSS.API.Clients.EprCommonDataApi;
using EPR.Calculator.FSS.API.Controllers;
using EPR.Calculator.FSS.API.Models;
using FluentAssertions;
using Json.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.FSS.API.UnitTests.Schemas;

[TestClass]
public class OrganisationsDetailsSchemaTests
{
    private const string UpdateSchemaEnvVar = "UPDATE_SCHEMAS";

    /// <summary>The checked-in schema, as copied to the test output directory.</summary>
    private static readonly string OutputSchemaPath = Path.Combine("Schemas", OrganisationsDetailsSchemaGenerator.FileName);

    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Fails if the checked-in schema is out of date with the response models.
    /// Run with UPDATE_SCHEMAS=1 to regenerate src/Schemas/organisations-details.schema.json.
    /// </summary>
    [TestMethod]
    public async Task CheckedInSchema_MatchesGeneratedSchema()
    {
        var generated = OrganisationsDetailsSchemaGenerator.Generate();

        if (Environment.GetEnvironmentVariable(UpdateSchemaEnvVar) == "1")
        {
            await File.WriteAllTextAsync(SourceSchemaPath(), generated, TestContext.CancellationToken);
            await File.WriteAllTextAsync(OutputSchemaPath, generated, TestContext.CancellationToken);
        }

        var checkedIn = await File.ReadAllTextAsync(OutputSchemaPath, TestContext.CancellationToken);

        checkedIn.ReplaceLineEndings("\n").Should().Be(
            generated,
            $"src/Schemas/{OrganisationsDetailsSchemaGenerator.FileName} is out of date - rerun the tests with {UpdateSchemaEnvVar}=1 to regenerate it");
    }

    [TestMethod]
    public async Task Response_FullyPopulated_ConformsToSchema()
    {
        var json = await GetResponseJson(
            CreateOrganisation("1", withOptionalFields: true, CreateSubsidiary("S1"), CreateSubsidiary("S2")),
            CreateOrganisation("2", withOptionalFields: true));

        AssertConforms(json);
    }

    [TestMethod]
    public async Task Response_OptionalFieldsNull_ConformsToSchema()
    {
        var json = await GetResponseJson(CreateOrganisation("1", withOptionalFields: false));

        AssertConforms(json);
    }

    [TestMethod]
    public async Task Response_Empty_ConformsToSchema()
    {
        AssertConforms(await GetResponseJson());
    }

    [TestMethod]
    public async Task Schema_RejectsMissingRequiredProperty()
    {
        var json = JsonNodeFromResponse(await GetResponseJson(CreateOrganisation("1", withOptionalFields: true)));
        json["organisationsDetails"]![0]!.AsObject().Remove("organisationId");

        Evaluate(json.ToJsonString()).IsValid.Should().BeFalse();
    }

    [TestMethod]
    public async Task Schema_RejectsInvalidFinancialYear()
    {
        var json = JsonNodeFromResponse(await GetResponseJson(CreateOrganisation("1", withOptionalFields: true)));
        json["organisationsDetails"]![0]!["financialYear"] = "2025";

        Evaluate(json.ToJsonString()).IsValid.Should().BeFalse();
    }

    [TestMethod]
    public async Task Schema_RejectsNullForNonNullableProperty()
    {
        var json = JsonNodeFromResponse(await GetResponseJson(CreateOrganisation("1", withOptionalFields: true)));
        json["organisationsDetails"]![0]!["organisationId"] = null;

        Evaluate(json.ToJsonString()).IsValid.Should().BeFalse();
    }

    private static System.Text.Json.Nodes.JsonNode JsonNodeFromResponse(string json) =>
        System.Text.Json.Nodes.JsonNode.Parse(json)!;

    private static void AssertConforms(string json)
    {
        var result = Evaluate(json);
        result.IsValid.Should().BeTrue(FormatErrors(result));
    }

    private static EvaluationResults Evaluate(string json)
    {
        var schema = JsonSchema.FromText(File.ReadAllText(OutputSchemaPath));

        return schema.Evaluate(
            JsonDocument.Parse(json).RootElement,
            new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });
    }

    /// <summary>
    /// Calls the controller and serializes its result the same way ASP.NET Core does.
    /// </summary>
    private static async Task<string> GetResponseJson(params OrganisationDetails[] organisations)
    {
        var client = new Mock<IEprCommonDataApiClient>();
        client
            .Setup(c => c.GetBillingOrganisations(It.IsAny<DateTimeOffset?>(), It.IsAny<FinancialYear?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisations);

        var controller = new OrganisationsController(client.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        var result = await controller.GetOrganisationsDetails(approvedAfter: null, financialYear: null);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        return JsonSerializer.Serialize(ok.Value, ok.Value!.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static OrganisationDetails CreateOrganisation(
        string id,
        bool withOptionalFields,
        params SubsidiaryDetails[] subsidiaries) =>
        new()
        {
            OrganisationId = id,
            FinancialYear = "2025-26",
            ApprovedDate = new DateTime(2025, 6, 1, 12, 30, 0, DateTimeKind.Utc),
            OrganisationName = withOptionalFields ? "Test Org" : null,
            OrganisationTradingName = withOptionalFields ? "Test Trading" : null,
            CompaniesHouseNumber = withOptionalFields ? "12345678" : null,
            HomeNationCode = withOptionalFields ? "EN" : null,
            ServiceOfNoticeAddrLine1 = withOptionalFields ? "1 Test Street" : null,
            ServiceOfNoticeAddrLine2 = withOptionalFields ? "Suite 100" : null,
            ServiceOfNoticeAddrCity = withOptionalFields ? "Test City" : null,
            ServiceOfNoticeAddrCounty = withOptionalFields ? "Test County" : null,
            ServiceOfNoticeAddrCountry = withOptionalFields ? "England" : null,
            ServiceOfNoticeAddrPostcode = withOptionalFields ? "TE1 1ST" : null,
            ServiceOfNoticeAddrPhoneNumber = withOptionalFields ? "01234567890" : null,
            SoleTraderFirstName = withOptionalFields ? "John" : null,
            SoleTraderLastName = withOptionalFields ? "Smith" : null,
            SoleTraderPhoneNumber = withOptionalFields ? "07123456789" : null,
            SoleTraderEmail = withOptionalFields ? "john.smith@test.com" : null,
            PrimaryContactPersonFirstName = withOptionalFields ? "Jane" : null,
            PrimaryContactPersonLastName = withOptionalFields ? "Doe" : null,
            PrimaryContactPersonPhoneNumber = withOptionalFields ? "07987654321" : null,
            PrimaryContactPersonEmail = withOptionalFields ? "jane.doe@test.com" : null,
            SubsidiaryDetails = [.. subsidiaries],
        };

    private static SubsidiaryDetails CreateSubsidiary(string id) =>
        new()
        {
            SubsidiaryId = id,
            SubsidiaryName = "Test Subsidiary",
            SubsidiaryTradingName = null,
            FinancialYear = "2025-26",
            ApprovedDate = new DateTime(2025, 6, 1, 12, 30, 0, DateTimeKind.Utc),
        };

    private static string SourceSchemaPath([CallerFilePath] string callerFilePath = "") =>
        Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(callerFilePath)!, "..", "..", "Schemas", OrganisationsDetailsSchemaGenerator.FileName));

    private static string FormatErrors(EvaluationResults result)
    {
        var lines = new List<string>();
        Collect(result, lines);
        return string.Join("\n", lines);
    }

    private static void Collect(EvaluationResults result, List<string> lines)
    {
        if (result.IsValid)
        {
            return;
        }

        foreach (var (keyword, message) in result.Errors ?? new Dictionary<string, string>())
        {
            lines.Add($"{result.InstanceLocation} [{keyword}]: {message}");
        }

        foreach (var detail in result.Details ?? [])
        {
            Collect(detail, lines);
        }
    }
}
