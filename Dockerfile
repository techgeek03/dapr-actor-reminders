FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

ARG VERSION

COPY Directory.Build.props .
COPY nuget.config . 
COPY Stylecop.json .
COPY .editorconfig .
COPY stylecop.ruleset.xml .
COPY *.sln .

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/${file}; done

RUN dotnet restore

COPY src/. ./src/
WORKDIR /src

RUN dotnet build --no-restore -c Release Dapr.Testing.sln

FROM build as publish-web-api
RUN dotnet publish --no-restore -c Release -o out src/Dapr.Testing.WebApi/Dapr.Testing.WebApi.csproj

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
    libc6-dev \
    libgdiplus \
    libx11-dev \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080;http://+:8081
EXPOSE 8080
EXPOSE 8081
WORKDIR /app

FROM runtime AS web-api
COPY --from=publish-web-api /src/out ./
ENTRYPOINT ["dotnet", "Dapr.Testing.WebApi.dll"]