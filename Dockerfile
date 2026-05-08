# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar caché
COPY ["LegalSaaS.Web/LegalSaaS.Web.csproj", "LegalSaaS.Web/"]
COPY ["LegalSaaS.Application/LegalSaaS.Application.csproj", "LegalSaaS.Application/"]
COPY ["LegalSaaS.Domain/LegalSaaS.Domain.csproj", "LegalSaaS.Domain/"]
COPY ["LegalSaaS.Infrastructure/LegalSaaS.Infrastructure.csproj", "LegalSaaS.Infrastructure/"]
COPY ["LegalSaaS.Shared/LegalSaaS.Shared.csproj", "LegalSaaS.Shared/"]

RUN dotnet restore "LegalSaaS.Web/LegalSaaS.Web.csproj"

# Copiar todo el código
COPY . .

# Publicar
WORKDIR /src/LegalSaaS.Web
RUN dotnet publish "LegalSaaS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Render usa el puerto 8080 por defecto
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LegalSaaS.Web.dll"]
