FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY InsuranceUnderwriting.sln .
COPY src/Shared/InsuranceUnderwriting.Contracts/InsuranceUnderwriting.Contracts.csproj src/Shared/InsuranceUnderwriting.Contracts/
COPY src/Write/InsuranceUnderwriting.Write.Domain/InsuranceUnderwriting.Write.Domain.csproj src/Write/InsuranceUnderwriting.Write.Domain/
COPY src/Write/InsuranceUnderwriting.Write.Application/InsuranceUnderwriting.Write.Application.csproj src/Write/InsuranceUnderwriting.Write.Application/
COPY src/Write/InsuranceUnderwriting.Write.Infrastructure/InsuranceUnderwriting.Write.Infrastructure.csproj src/Write/InsuranceUnderwriting.Write.Infrastructure/
COPY src/Write/InsuranceUnderwriting.Write.Api/InsuranceUnderwriting.Write.Api.csproj src/Write/InsuranceUnderwriting.Write.Api/
RUN dotnet restore src/Write/InsuranceUnderwriting.Write.Api/InsuranceUnderwriting.Write.Api.csproj

COPY src/Shared/InsuranceUnderwriting.Contracts/ src/Shared/InsuranceUnderwriting.Contracts/
COPY src/Write/ src/Write/
RUN dotnet publish src/Write/InsuranceUnderwriting.Write.Api/InsuranceUnderwriting.Write.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "InsuranceUnderwriting.Write.Api.dll"]
