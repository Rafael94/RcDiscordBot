FROM mcr.microsoft.com/dotnet/runtime:5.0 
ARG PUBLISH_PATH
WORKDIR /app
COPY $PUBLISH_PATH .
ENTRYPOINT ["dotnet", "RcDiscordBot.dll"]
