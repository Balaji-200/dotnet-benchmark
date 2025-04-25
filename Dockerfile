FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /app

COPY Benchmark.csproj Benchmark.csproj

RUN dotnet restore

COPY . .

ENTRYPOINT ["dotnet", "run", "-c", "Release"]