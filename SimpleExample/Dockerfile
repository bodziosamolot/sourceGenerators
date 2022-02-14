FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SimpleExample/SimpleExample.csproj", "SimpleExample/"]
RUN dotnet restore "SimpleExample/SimpleExample.csproj"
COPY . .
WORKDIR "/src/SimpleExample"
RUN dotnet build "SimpleExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleExample.dll"]
