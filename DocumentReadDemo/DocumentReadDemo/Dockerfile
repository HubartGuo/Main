FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DocumentViewer.csproj", "."]
RUN dotnet restore "DocumentViewer.csproj"
COPY . .
RUN dotnet build "DocumentViewer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentViewer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/Documents"]
ENTRYPOINT ["dotnet", "DocumentViewer.dll"]