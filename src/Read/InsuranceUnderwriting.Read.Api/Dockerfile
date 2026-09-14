FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY InsuranceUnderwriting.sln .
COPY src/Shared/InsuranceUnderwriting.Contracts/InsuranceUnderwriting.Contracts.csproj src/Shared/InsuranceUnderwriting.Contracts/
COPY src/Read/InsuranceUnderwriting.Read.Domain/InsuranceUnderwriting.Read.Domain.csproj src/Read/InsuranceUnderwriting.Read.Domain/
COPY src/Read/InsuranceUnderwriting.Read.Infrastructure/InsuranceUnderwriting.Read.Infrastructure.csproj src/Read/InsuranceUnderwriting.Read.Infrastructure/
COPY src/Read/InsuranceUnderwriting.Read.Api/InsuranceUnderwriting.Read.Api.csproj src/Read/InsuranceUnderwriting.Read.Api/
RUN dotnet restore src/Read/InsuranceUnderwriting.Read.Api/InsuranceUnderwriting.Read.Api.csproj

COPY src/Shared/InsuranceUnderwriting.Contracts/ src/Shared/InsuranceUnderwriting.Contracts/
COPY src/Read/ src/Read/
RUN dotnet publish src/Read/InsuranceUnderwriting.Read.Api/InsuranceUnderwriting.Read.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "InsuranceUnderwriting.Read.Api.dll"]
