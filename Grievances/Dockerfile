FROM mcr.microsoft.com/dotnet/aspnet:2.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:2.1 AS build
WORKDIR /src
COPY ["Grievances/GrievanceService.csproj", "Grievances/"]
RUN dotnet restore "Grievances/GrievanceService.csproj"
COPY . .
WORKDIR "/src/Grievances"
RUN dotnet build "GrievanceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrievanceService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN mkdir EmailTemplates
COPY ./Grievances/EmailTemplates ./EmailTemplates
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrievanceService.dll"]
