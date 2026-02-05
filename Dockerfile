# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY Zubac/*.csproj ./Zubac/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/Zubac
RUN dotnet publish -c Release -o out

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Zubac/out .
ENTRYPOINT ["dotnet", "Zubac.dll"]