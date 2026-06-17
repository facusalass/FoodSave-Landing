FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Foodsave.Web/Foodsave.Web.csproj Foodsave.Web/
RUN dotnet restore Foodsave.Web/Foodsave.Web.csproj

COPY . .
RUN dotnet publish Foodsave.Web/Foodsave.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Foodsave.Web.dll"]
