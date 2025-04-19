FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /app

COPY Benchmark.csproj Benchmark.csproj

RUN dotnet restore

COPY . .

RUN dotnet build -c Release

ENTRYPOINT ["dotnet", "run", "-c", "Release", "-r", "linux-x64", "--", "--runtime", "net6.0"]