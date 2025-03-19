FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
RUN ln -s ~/.dotnet/tools/libman /usr/local/bin/libman

WORKDIR /src
COPY ["src/MyProject.Web.Mvc/MyProject.Web.Mvc.csproj", "src/MyProject.Web.Mvc/"]
COPY ["src/MyProject.Web.Core/MyProject.Web.Core.csproj", "src/MyProject.Web.Core/"]
COPY ["src/MyProject.Application/MyProject.Application.csproj", "src/MyProject.Application/"]
COPY ["src/MyProject.Core/MyProject.Core.csproj", "src/MyProject.Core/"]
COPY ["src/MyProject.EntityFrameworkCore/MyProject.EntityFrameworkCore.csproj", "src/MyProject.EntityFrameworkCore/"]
WORKDIR "/src/src/MyProject.Web.Mvc"
RUN dotnet restore 

WORKDIR /src
COPY ["src/MyProject.Web.Mvc", "src/MyProject.Web.Mvc"]
COPY ["src/MyProject.Web.Core", "src/MyProject.Web.Core"]
COPY ["src/MyProject.Application", "src/MyProject.Application"]
COPY ["src/MyProject.Core", "src/MyProject.Core"]
COPY ["src/MyProject.EntityFrameworkCore", "src/MyProject.EntityFrameworkCore"]
WORKDIR "/src/src/MyProject.Web.Mvc"
RUN libman restore
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "MyProject.Web.Mvc.dll"]
