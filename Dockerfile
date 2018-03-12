FROM microsoft/aspnetcore:2.0.0
WORKDIR /app
COPY ./artifacts .
EXPOSE 5000
ENTRYPOINT ["dotnet", "ProductContext.WebApi.dll"]