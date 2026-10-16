# CareNova monitoring

Prometheus and Grafana run in Docker on `carenova-kafka-vm` (`/home/azureuser/monitoring`).
Prometheus is on port 9090 and Grafana on port 3000.

Every service exposes `/metrics` through `prometheus-net.AspNetCore`.

- `prometheus.yml`: the scrape config the VM uses. It covers the gateway, appointment-integration, doctor-payment, pharmacy-order and user-auth.
- `grafana-carenova-dashboard.json`: the "CareNova Services" dashboard. It shows up/down, request rate, 5xx errors, p95 latency, in-flight requests, top endpoints, memory and CPU for each service.

## Updating the scrape config

Copy `prometheus.yml` to `/home/azureuser/monitoring/prometheus.yml` on the VM, then run `docker restart carenova-prometheus`.

## Importing the dashboard

In Grafana, go to Dashboards → New → Import. Upload `grafana-carenova-dashboard.json` and pick the Prometheus data source.
