FROM mcr.microsoft.com/dotnet/runtime:5.0

COPY bin/executable/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "mtcg.dll"]
