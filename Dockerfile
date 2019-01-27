FROM microsoft/dotnet:2.2-runtime
WORKDIR /app

ENV LANG en_US.UTF-8  
ENV LANGUAGE en_US:en  
ENV LC_ALL en_US.UTF-8 
ENV TZ=America/Toronto

COPY bin/Debug/netcoreapp2.1/publish .

ENTRYPOINT dotnet CarSearch.dll