#!zsh

docker compose build

docker tag dapr-actor-reminders_dapr-testing-api dweurelaylocaldevacr.azurecr.io/dapr/testing-web-api
docker tag dapr-actor-reminders_dapr-actors-runtime dweurelaylocaldevacr.azurecr.io/dapr/testing-actors-runtime

docker push dweurelaylocaldevacr.azurecr.io/dapr/testing-web-api
docker push dweurelaylocaldevacr.azurecr.io/dapr/testing-actors-runtime

docker image prune

kubectl apply -f k8s/app/ -n apps
kubectl rollout restart deployments testing-actor-runtime -n apps
kubectl rollout restart deployments testing-web-api -n apps