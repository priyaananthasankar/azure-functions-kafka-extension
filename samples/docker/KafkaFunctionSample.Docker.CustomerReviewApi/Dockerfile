FROM microsoft/dotnet:2.2-sdk AS installer-env

RUN mkdir -p /app
COPY ./src.ruleset /app
COPY ./stylecop.json /app
COPY ./samples/docker/KafkaFunctionSample.Docker.CustomerReviewApi /app/samples/docker/KafkaFunctionSample.Docker.CustomerReviewApi
COPY ./samples/docker/KafkaFunctionSample.Docker.Common /app/samples/docker/KafkaFunctionSample.Docker.Common
COPY ./build /app/build
COPY ./src/Microsoft.Azure.WebJobs.Extensions.Kafka /app/src/Microsoft.Azure.WebJobs.Extensions.Kafka

RUN cd /app/samples/docker/KafkaFunctionSample.Docker.CustomerReviewApi && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj /p:GenerateRuntimeConfigurationFiles=true --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

RUN apt-get update && apt-get install -y wget gnupg apt-transport-https
RUN wget -qO - https://packages.confluent.io/deb/5.2/archive.key | apt-key add -
RUN apt-get install -y software-properties-common
RUN add-apt-repository "deb [arch=amd64] https://packages.confluent.io/deb/5.2 stable main"
RUN add-apt-repository "deb http://security.debian.org/debian-security jessie/updates main"

RUN apt-get update && apt install -y librdkafka-dev 

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]