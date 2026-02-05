# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY <ime_projekta>/*.csproj ./<ime_projekta>/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/<ime_projekta>
RUN dotnet publish -c Release -o out

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/<ime_projekta>/out .
ENTRYPOINT ["dotnet", "<ime_projekta>.dll"]