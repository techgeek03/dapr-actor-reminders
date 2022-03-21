# Dapr Actors Reminders

Project for testing dapr actor reminders.

## Deploy Dapr in Kubernetes

Make sure you are always operating in the root of this repo.

```
helm repo add dapr https://dapr.github.io/helm-charts/
helm repo update
kubectl create ns dapr-system
helm install dapr dapr/dapr --namespace dapr-system --values k8s/dapr/values.yaml --wait
```

## Deploy app in Kubernetes

```
kubectl create ns apps

```
