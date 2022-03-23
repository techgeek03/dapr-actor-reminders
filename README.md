# Dapr Actors Reminders

Project for testing dapr actor reminders.

## Pre-requirements

1. Kubernetes cluster (This demo uses AKS 3 node cluster)
2. Container Registry (This demo uses ACR)
3. Azure CosmosDB for actor state store

## High level components architecture

```ascii
        ┌──────────────────┐
        │                  │
        │     Ingress      │
        │                  │
        │                  │
        └─────────┬────────┘
                  │
                  │
        ┌─────────▼────────┐
        │                  │
        │   Test Web Api   │
        │                  │
        │   Invokes dapr   │
        └─────────┬────────┘
                  │
                  │
    ┌─────────────▼────────────────┐
    │   Test Actor Runtime         │
    │                              │
    │ Hosts actors:                │
    │ - RemindMeEveryMinute01      │
    │ - RemindMeEveryMinute02      │
    │                              │
    │                              │
    │                              │
    │                              │
    └─────────────▲┌───────────────┘
                  ││
                  ││
┌─────────────────┘▼──────────────────────┐
│                                         │
│                                         │
│         DAPR ACTOR STATE STORE          │
│                                         │
│                                         │
└─────────────────┬─▲─────────────────────┘
                  │ │
             ┌────▼─┴──────┐
             │             │
             │   AZURE     │
             │             │
             │   COSMOSDB  │
             └─────────────┘
```

## Build and Deploy

Make sure you are always operating in the root of this repo.

### Deploy Dapr in Kubernetes

Dapr values are configured in `k8s/dapr/values.yaml`. Use this file to tweak the dapr deployment if needed.

```bash
helm repo add dapr https://dapr.github.io/helm-charts/
helm repo update
kubectl create ns dapr-system
helm install dapr dapr/dapr --namespace dapr-system --values k8s/dapr/values.yaml --wait
kubectl create ns apps
```

### Build app for deployment

Build the app consist of preparing an container image build with Docker that is pushed to an registry

```bash
docker compose build

docker tag dapr-actor-reminders_dapr-testing-api <Your registry name>/dapr/testing-web-api
docker tag dapr-actor-reminders_dapr-actors-runtime  <Your registry name>/dapr/testing-actors-runtime

docker push <Your registry name>/dapr/testing-web-api
docker push <Your registry name>/dapr/testing-actors-runtime
```

### Build and Deploy app in Kubernetes

There is script that will do all necessary steps to build and deploy the app. You must pass the name of the container registry as parameter.

```bash
./build.sh <Your registry name>
```
