FROM mcr.microsoft.com/dotnet/runtime:5.0 
ARG PUBLISH_PATH
WORKDIR /app
COPY $PUBLISH_PATH .
VOLUME ["/app/appsettings.json"]
ENTRYPOINT ["dotnet", "RcDiscordBot.dll"]