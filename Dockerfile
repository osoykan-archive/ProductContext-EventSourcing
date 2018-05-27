FROM microsoft/aspnetcore-build AS build-env
WORKDIR /workdir
COPY . /workdir
 
RUN dotnet restore ./ProductContext.sln
RUN dotnet test    ./test/ProductContext.Domain.Tests/ProductContext.Domain.Tests.csproj
RUN dotnet test    ./test/ProductContext.Framework.Tests/ProductContext.Framework.Tests.csproj
RUN dotnet publish ./src/ProductContext.WebApi/ProductContext.WebApi.csproj -o /publish -c Release -r linux-x64

FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=build-env ./publish .

ENV ASPNETCORE_ENVIRONMENT="Development"
ENV ASPNETCORE_URLS="http://*:5000"

EXPOSE 5000
ENTRYPOINT ["dotnet", "ProductContext.WebApi.dll"]