#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.22 AS base

RUN useradd app -u 10001 --create-home --user-group
USER 10001

WORKDIR /app
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OrderMicroservice/Order.API/Order.API.csproj", "OrderMicroservice/Order.API/"]
RUN dotnet restore "OrderMicroservice/Order.API/Order.API.csproj"
COPY . .
WORKDIR "/src/OrderMicroservice/Order.API"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]