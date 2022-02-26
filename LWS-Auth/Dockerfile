FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS BUILDER
RUN apt-get update
RUN apt-get install curl gnupg lsb-release -y
RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
RUN mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
RUN sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'
RUN apt-get update
RUN apt-get install azure-functions-core-tools-4 -y

COPY . /src/dotnet-function-app
RUN rm /src/dotnet-function-app/local.settings.json
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish --configuration Debug *.csproj --output /home/site/wwwroot 

WORKDIR /home/site/wwwroot
ENTRYPOINT ["/usr/bin/func", "start", "--no-build", "--verbose"]