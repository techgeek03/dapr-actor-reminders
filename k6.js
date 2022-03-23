import http from "k6/http";

export let options = {
    vus: 5,
    stages: [
        { duration: "10s", target: 20 },
        { duration: "2m", target: 200 }
    ]
};

export default function() {
    let response = http.get("http://localhost:8080/api/actor-reminders-tests/test01");
};