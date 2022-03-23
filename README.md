# Dapr Actors on dotNet

Project for testing dapr actors with dotNet SDK.

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
        │   Invokes dapr   │
        │      actor       │
        └─────────┬────────┘
                  │
                  │
    ┌─────────────▼────────────────┐
    │   Test Actor Runtime         │
    │                              │
    │ Hosts actor types:           │
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

You will need to create the actor state store dapr component as well. The configuration bellow is not recommended since the secret is added in plain text, but since this is for testing purposes will allow it :) Refer to official documentation for production configuration. In CosmosDB make sure you have created the database `dapr-store` and added collection `actorStateStore`.

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: dapr-actor-state-store
spec:
  type: state.azure.cosmosdb
  version: v1
  metadata:
    - name: url
      value: "<Replace with the url of CosmosDB>"
    - name: masterKey
      value: "<Replace with the master key for CosmosDB>"
    - name: database
      value: dapr-store
    - name: collection
      value: actorStateStore
    - name: actorStateStore
      value: "true"
```

Create the component in kubernetes:

```bash
kubectl apply -f k8s/app/state-store.yaml -n apps
```

### Prepare app for deployment

Building the app consist of preparing an container image build with Docker that is pushed to an container registry. Ensure that you have logged in to your registry first.

For ACR (Azure Container registry)

```bash
az acr login --name <Your registry name>
```

For Docker Hub

```bash
docker login
```

Next build

```bash
docker compose build

docker tag dapr-actor-reminders_dapr-testing-api <Your registry name>/dapr/testing-web-api
docker tag dapr-actor-reminders_dapr-actors-runtime  <Your registry name>/dapr/testing-actors-runtime

docker push <Your registry name>/dapr/testing-web-api
docker push <Your registry name>/dapr/testing-actors-runtime
```

### Deploy app

The deployment of the app is consistent of two kubernetes deployments and two services. The location of the files is in `k8s/app/`. Before deploying make sure you use the correct name of your container registry of the image. Modify [actor-runtime.yaml](k8s/app/actor-runtime.yaml) and [web-api.yaml](web-api.yaml) deployments with

```yaml
image: <Your registry name>/dapr/testing-actors-runtime
```

Then deploy with

```bash
kubectl apply -f k8s/app/ -n apps
```

### Build and Deploy app in Kubernetes

There is script that will do all necessary steps to build and deploy the app. You must pass the name of the container registry as parameter.

```bash
./build.sh <Your registry name>
```

## Dapr Bugs

1. When reminders are used for actors; under certain conditions single actor instance is placed in two pods. More [here --->](bugs/01/BUG01.md).
