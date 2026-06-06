FROM node:24-alpine AS frontend
WORKDIR /src/front/UniFlowPeople.App

COPY front/UniFlowPeople.App/package*.json ./
RUN npm ci

COPY front/UniFlowPeople.App/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY back/UniFlowPeople.Api/UniFlowPeople.Api.csproj back/UniFlowPeople.Api/
RUN dotnet restore back/UniFlowPeople.Api/UniFlowPeople.Api.csproj

COPY back/ back/
RUN dotnet publish back/UniFlowPeople.Api/UniFlowPeople.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:BuildFrontend=false

RUN mkdir -p /app/publish/wwwroot
COPY --from=frontend /src/front/UniFlowPeople.App/dist/uniflowpeople-front/browser/ /app/publish/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UniFlowPeople.Api.dll"]
