# When actor is executing long running operation single instance of the actor is executing two actor methods in parallel in two different pods

When actor is executing long running operation (5+ seconds), under certain circumstances; single instance of the actor can be executing two methods in parallel in two different pods. "under certain circumstances" in this case means the following:

1. Dapr runtime deployed in Kubernetes with HA enabled.
2. Actor with Actor ID has been created and is placed on a single pod.
3. Actor method has been invoked and is taking long time for this operation to complete. (RPC operation to external service, database call, etc.)
4. While the method is still running; the pod where this actor is placed is evicted/killed by kubernetes or user or just by redeploying the application.
5. The same or other actor method is called while the first method is still running.

There are two problems (bugs) now:

1. The actor with the same instance (Actor ID) now is been placed on other pod and has started invoking the requested method while the first method is still running.
2. Since **daprd** sidecar received the **SIGTERM** signal from kubernetes it has shout down the http endpoints and is not receiving any requests. However since the first method is still running (imagine external http call and slow network) the next step in that method can be to use the actor state store or maybe publish event; which are using the dapr SDK; and this will fail since **daprd** is no longer receiving requests.

## Project description

From the high level component architecture displayed [here](../../README.md#high-level-components-architecture) we can see that we are having two deployable units:

- Rest API that is exposed thru an ingress and receives requests. The API uses the Dapr SDK and the Dapr Actor Proxy to invoke actors. (Dapr.Testing.WebApi.csproj)
- The dapr actor runtime that hosts the actors. (Dapr.Testing.Actors.Runtime.csproj)

In the actor runtime project we have an actor with type `InvokeExternalEndpointWithDelay` and is implement in the file [InvokeExternalEndpointWithDelay.cs](../../src/Dapr.Testing.Actors.Runtime/InvokeExternalEndpointWithDelayActor.cs). It is a simple actor that has only one method `InvokeEndpoint` that is invoking an http request to <https://deelay.me/60000/https://google.com/> which simulates long running process and blocks the execution for 60 seconds. Once the request is completed the actor is trying to persist the status of that operation by using the actor state store.

The dapr actor runtime project (Dapr.Testing.Actors.Runtime.csproj) configures Kestrel (the ASP.Net http server) to wait for 60 seconds for all outstanding HTTP requests to finish before it allows to be shutdown which can be seen in [Program.cs](../../src/Dapr.Testing.Actors.Runtime/Program.cs) This means that once the container receives the **SIGTERM** from Kubernetes it will wait for maximum of 60 seconds for requests to complete.

```csharp
builder.Host.ConfigureHostOptions(o => o.ShutdownTimeout = TimeSpan.FromSeconds(60));
```

Further more the Kubernetes deployment of the application hosting the actor runtime is configured in this deployment file [actor-runtime.yaml](../../k8s/app/actor-runtime.yaml) with the following configuration (the deployment shown below is only part of the file, used to showcase the important settings):

```yaml
spec:
  progressDeadlineSeconds: 600
  replicas: 3
  revisionHistoryLimit: 10
  selector:
    matchLabels:
      app: testing-actor-runtime
      version: v1
  template:
    metadata:
      annotations:
        dapr.io/graceful-shutdown-seconds: "60"
  strategy:
    rollingUpdate:
      maxSurge: 30%
      maxUnavailable: 25%
    type: RollingUpdate
  template:
    spec:
      containers:
        terminationGracePeriodSeconds: 60
```

As you can see we have configured `terminationGracePeriodSeconds` to instruct Kubernetes to wait for 60 seconds until our host completes all requests as well as added annotation to dapr sidecar `dapr.io/graceful-shutdown-seconds: "60"`.

## Steps to reproduce the problem

1. Make sure you have this repo cloned and the apps are deployed as explained in the [readme](../../README.md) file.
2. Port forward any of the `testing-web-api` pods. kubectl port-forward deployment/testing-web-api 8080:8080 -n apps
3. You will have about 60 seconds to complete steps 4-6 so be prepared :). (Makes sure you have two terminal session open and ready.)
4. curl -X POST -v http://localhost:8080/api/actor-tests/invoke-external-endpoint/actorWithLongTimeToProcess
5. You need to be quick now to find and delete the pod where the actor is placed. For this you will need to look in the logs in all pods. Then run `kubectl delete pod <name-of-the-pod-where-the-actor-is-placed> -n apps`.
6. curl -X POST -v http://localhost:8080/api/actor-tests/invoke-external-endpoint/actorWithLongTimeToProcess
7. Now you should see that the actor is activated on new pod and has started to invoke the method, while the first method is running.
8. In the first pod once the invocation of the external call has completed you will find exception that the actor was unable to persist the state.

## Logs

We have logs from our runs that can be analyzed as well. They are located in [bugs/02/logs](logs). The logs are from the **daprd** sidecar as well as the application hosting the actor runtime.
