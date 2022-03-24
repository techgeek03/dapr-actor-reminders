import http from "k6/http";

export let options = {
    vus: 5,
    stages: [
        { duration: "30s", target: 20 },
        { duration: "1m", target: 200 }
    ]
};

export default function() {
    let response = http.get("http://localhost:8080/api/actor-tests/test01?secondsDelay=10&id=test1");
};