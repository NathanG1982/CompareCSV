FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY CompareCsv/CompareCsv.csproj CompareCsv/
RUN dotnet restore CompareCsv/CompareCsv.csproj
COPY . .
WORKDIR /src/CompareCsv
RUN dotnet build CompareCsv.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CompareCsv.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CompareCsv.dll"]
