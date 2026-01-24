# Backend Dockerfile (Multi-stage build)

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["API/*.csproj", "API/"]
COPY ["Application/*.csproj", "Application/"]
COPY ["Domain/*.csproj", "Domain/"]
COPY ["Infrastructure/*.csproj", "Infrastructure/"]

RUN dotnet restore "API/API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 5000

COPY --from=publish /app/publish .

# Wait for SQL Server to be ready
RUN apt-get update && apt-get install -y netcat-traditional

# Create entrypoint script
RUN echo '#!/bin/bash\n\
echo "Waiting for SQL Server..."\n\
while ! nc -z sqlserver 1433; do\n\
  sleep 1\n\
done\n\
echo "SQL Server is ready!"\n\
\n\
echo "Running migrations..."\n\
dotnet API.dll --migrate\n\
\n\
echo "Starting application..."\n\
dotnet API.dll' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
