# epr-calculator-fss-api
This repository is part of the wider EPR Payment Calculator solution

## How to run locally

This API calls `epr-calculator-api` and `epr-common-data-api`, so run both locally first (or point the config at a deployed environment).

```
cp src/EPR.Calculator.FSS.API/appsettings.template.jsonc src/EPR.Calculator.FSS.API/appsettings.local.json
```

`appsettings.local.json` is gitignored and loaded when `ASPNETCORE_ENVIRONMENT` is `local`, which the launch profiles set. Adjust its values as needed, or put them in your user secrets instead. In VS Code, you can open the secrets file by right-clicking `src/EPR.Calculator.FSS.API/EPR.Calculator.FSS.API.csproj` → *Manage User Secrets*.

```
dotnet run --project src/EPR.Calculator.FSS.API
```
