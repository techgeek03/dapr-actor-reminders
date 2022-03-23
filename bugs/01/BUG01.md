# When actor reminders are used single actor is assigned on two pods

We have noticed that when actor has reminder configured; under certain circumstances; single actor instance can be placed on two pods. Obviously this brakes the actor pattern and core actor principals. "under certain circumstances" in this case means the following:

0. Dapr runtime deployed in Kubernetes with HA enabled.
1. Actor with Actor ID has been created and is placed on a single pod. (The actor is still in memory, since the actor idle time has not expired).
2. Reminder is created for this actor that will invoke the reminder every minute.
3. The pod where this actor is placed is evicted/killed by kubernetes or user or just by redeploying the application.
4. When the reminder is executed the actor is activated on new pod. But also the same actor is activated on a second pod as well.

Important: Sometimes is hard to reproduce the problem so one need to try several times.

## Project description

From the high level component architecture displayed [here](../../README.md#high-level-components-architecture) we can see that we are having two deployable units:

- Rest API that is exposed thru an ingress and receives requests. The API uses the Dapr SDK and the Dapr Actor Proxy to invoke actors. (Dapr.Testing.WebApi.csproj)
- The dapr actor runtime that hosts the actors. (Dapr.Testing.Actors.Runtime.csproj)

In the actor runtime project we have an actor with type `RemindMeEveryMinute01` and is implement in the file [RemindMeEveryMinute01Actor.cs](../../src/Dapr.Testing.Actors.Runtime/RemindMeEveryMinute01Actor.cs). It is a simple actor that has only one method `RegisterReminder` that creates reminder with period of one minute; as well as the implementation of that reminder invocation. The implementation of that reminder uses the actor state store to increase the `Count` to the number of invocations of the reminder. After 10 invocations it deregister the reminder. Also an additional actor state store is added that stores the time and the name of the pod that this actor runs, for every reminder invocation. The purpose of this is to just log additional data in the store so we can debug things easier.

The deployment of the application hosting the actor runtime is configured in this deployment file [actor-runtime.yaml](../../k8s/app/actor-runtime.yaml) with the following configuration (the deployment shown below is only part of the file used to showcase the important settings):

```yaml
spec:
  progressDeadlineSeconds: 600
  replicas: 3
  revisionHistoryLimit: 10
  selector:
    matchLabels:
      app: testing-actor-runtime
      version: v1
  strategy:
    rollingUpdate:
      maxSurge: 30%
      maxUnavailable: 25%
    type: RollingUpdate
  template:
    spec:
      containers:
      - env:
        - name: Dapr__Actors__IdleTimeoutSeconds
          value: "300"
        - name: Dapr__Actors__ScanIntervalSeconds
          value: "30"
```

As you can see we are deploying replica with 3 pods with a rolling update strategy.
Further more we configured the Dapr Actor `ActorIdleTimeout` to 300 seconds and the `ActorScanInterval` to 30 seconds. Take a look at the Startup configuration here [Startup.cs](../../src/Dapr.Testing.Actors.Runtime/Startup.cs)

## Steps to reproduce the problem

1. Make sure you have this repo cloned and the apps are deployed as explained in the [readme](../../README.md) file.
2. Port forward any of the `testing-web-api` pods. kubectl port-forward deployment/testing-web-api 8080:8080 -n apps
3. curl -v http://localhost:8080/api/actor-reminders-tests/test01
4. Wait 2-4 minutes for the reminder to execute several times.
5. Delete the pod where the actor is placed. For this you will need to look in the logs in all pods. Then run `kubectl delete pod <name-of-the-pod-where-the-actor-is-placed> -n apps`.
6. Wait 2-3 for the reminder to execute several times. Check the pod logs. You should see that the actor is activated in two pods now.

*Important: Sometimes is hard to reproduce the problem so one need to try several times.*

## Logs

We have logs from our runs that can be analyzed as well. They are located in [bugs/01/logs](logs). The logs are from the **daprd** sidecar as well as the application hosting the actor runtime. The actor id that has been placed on 3 pods in this case is `d0ddd78d-e80d-4d61-b01c-706c45acfbd6` and can be found in files `testing-actor-runtime-1.log`, `testing-actor-runtime-2.log` and `testing-actor-runtime-3.log` and in the same time interval and the reminder has been called in all of them. In `testing-actor-runtime-1.log` and `testing-actor-runtime-3.log` one can search for the string "Reminder received" that will point that the reminder was called on two separate pods in the same time (technically not same only few milliseconds apparat).
