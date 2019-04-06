# Sample on Docker

Azure functions can be [hosted in a docker container](https://docs.microsoft.com/en-us/Azure/azure-functions/functions-create-function-linux-custom-image).

The demo application in this folder contains a simple product review application:

```text
    (1)                     (2)               (3)             (3)
 ________                 ________          ________        ________
|        |               |        |        |  Bad   |      |        |
| Review | ============> | Review | =====> | Review | ===> | Cosmos |
|  Api   | (all reviews) | Trigger| (bad   | Trigger|      |        |
|_______ |               |_______ |  ones) |_______ |      |_______ |
```

1. Review api Function takes a POST with a customer review. It posts to "customerreviews" Kafka topic
1. Review trigger Function consumes "customerreviews" and does very high-tech sentiment analysis (searchs for "bad"). Negative reviews are enchanced with country of origin based on IP address (again very high-tech; always USA). The resulting value is sent to topic "badcustomerreviews"
1. Bad review trigger Function consumes "badcustomerreviews", outputting to Cosmos DB.

To run the full application you will need:
1. An Azure subscription
1. A CosmosDB database named KafkaDemo with a collection named BadReviews.

## Running with docker-compose

On a command shell go to ./samples/docker and type:

```bash
# define the image version
export TAG 0.0.1

# set the cosmosdb connection string
export CosmosDB "<your-cosmos-db-connection-string>"

# start all containers of the sample application
docker-compose up --build -d
```

Wait until the containers are up and running.

Post a new review to api with curl:

```bash
curl -X POST -d '{ "productId": "2", "text": "bad", "ipaddress": "127.0.0.1", "timestamp": 0 }' http://localhost:5001/api/reviews
```

Now check in your Cosmos DB for negative reviews (i.e. `SELECT top 3 * FROM c ORDER BY c._ts DESC`).

## Kubernetes

The sample application can be deployed to a Kubernetes cluster. We will be using [Confluent's Kafka Helm Chart](https://docs.confluent.io/current/installation/installing_cp/cp-helm-charts/docs/index.html).

Requirements to run the sample application on Kubernetes are having a cluster with at least 3 nodes, ingress and Helm installed. If you create a new AKS cluster make sure Http application routing is activated for a quick ingress solution (the deployment files are based on [Http application routing](https://docs.microsoft.com/nb-no/azure/aks/http-application-routing)).

1. Add confluent helm repo\
`helm repo add confluentinc https://confluentinc.github.io/cp-helm-charts/`
1. Update helm repos\
`helm repo update`
1. Ensure you have a storage class available. The sample deployment is using `default`\
`kubectl get storageClass`
1. Deploy the Confluent Kafka helm to your cluster
```bash
helm install --name mykafka --set cp-schema-registry.enabled=false,cp-kafka-rest.enabled=false,cp-kafka-connect.enabled=false,dataLogDirStorageClass=default,dataDirStorageClass=default,storageClass=default confluentinc/cp-helm-charts
```
5. Wait until all pods have been created (3x kafka, 3x zookeeper). It takes around 10min to be ready. To verify run:
```bash
kubectl get po
NAME                                      READY   STATUS    RESTARTS   AGE
mykafka-cp-kafka-0                        2/2     Running   0          10m
mykafka-cp-kafka-1                        2/2     Running   0          5m54s
mykafka-cp-kafka-2                        2/2     Running   0          5m17s
mykafka-cp-ksql-server-64b6b54b86-rfdnh   2/2     Running   6          10m
mykafka-cp-zookeeper-0                    2/2     Running   0          10m
mykafka-cp-zookeeper-1                    2/2     Running   0          9m9s
mykafka-cp-zookeeper-2                    2/2     Running   0          8m16s
```

6. Create the required Kafka topics: `customerreviews` and `badcustomerreviews`
```bash
kubectl exec -c cp-kafka-broker -it mykafka-cp-kafka-0 -- /bin/bash /usr/bin/kafka-topics --zookeeper mykafka-cp-zookeeper:2181 --create --topic customerreviews --partitions 12 --replication-factor 1 --if-not-exists

kubectl exec -c cp-kafka-broker -it mykafka-cp-kafka-0 -- /bin/bash /usr/bin/kafka-topics --zookeeper mykafka-cp-zookeeper:2181 --create --topic badcustomerreviews --partitions 12 --replication-factor 1 --if-not-exists
```
7. If you haven't yet, create a CosmosDB database named `KafkaDemo` with a collection named `BadReviews`
8. Modify the file `./samples/docker/k8s-deployment.yaml` replacing the placeholder values `<COSMOS-DB-CONNECTION-STRING>` and `<CLUSTER_SPECIFIC_DNS_ZONE>`
9. Deploy sample application: `kubectl apply -f ./samples/docker/k8s-deployment.yaml`
10. Wait a while until the DNS has been updated. Navigating to `http://reviewapi.{number}.{region}.aksapp.io` should show the Azure Functions welcome page
11. Test the application by sending a review
```bash
curl -X POST -d '{ "productId": "2", "text": "bad", "ipaddress": "127.0.0.1", "timestamp": 0 }' http://reviewapi.<your-aks-dns>.<your-aks-region>.aksapp.io/api/reviews
```
12. Check your CosmosDB for negative reviews (i.e. `SELECT top 3 * FROM c ORDER BY c._ts DESC`)
13. Change the replicas count for each trigger and verify that the partitions will be redistributed. To verify
run `kubectl logs <pod-identifier-of-trigger>`. You should see something like this:
```log
info: Host.Triggers.Kafka[0]
      Revoked partitions: [customerreviews [[0]] @Unset [-1001], customerreviews [[1]] @7, customerreviews [[2]] @Unset [-1001], customerreviews [[3]] @Unset [-1001]]
info: Host.Triggers.Kafka[0]
      Assigned partitions: [customerreviews [[0]], customerreviews [[1]]]
```