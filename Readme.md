# Performance test med Kubernetes

## TestService-folderen

Dette repository indeholder et simpelt AspNet Core WebAPI projekt, baseret på en standard WebAPI template. Projektet findes i foldere TestService. 

- Servicen er forberedt på at kunne bygges til en container med en `Dockerfile` som ligger i projektet.
- Projektet har en `NLog.config` som kan logge til konsolet en fil og Loki
- Programmet anvender OpenTelemetry biblioteker til at opsamle data som kan høstes af en `Prometheus-server`

---

## Staging-folderen

For at afprøve projektet fra i et `Docker runtime` er der oprettet en `Staging`-folder med en `docker-compose.ym`. 
For at bygge containeren og køre projektet køres følgende kommando fra en terminal:

```bash
 $ docker compose build 
 $ docker compose up -d
```

Skal det byggede image push'es til et DockerHub repository kræver det at kontonavnet for imaget omdøbes inden det bygges og pushes med:

```bash
 $ docker compose build push
```

`Staging`-folderen indeholder også et bash-script som anvender `curl` til at kalde endepunkterne i servicen og dermed aktivere de .NET `http-metrics` som er aktiveret i servicen.

---

## Minikube-Env-folderen

Denne folder indeholder de nødvendige filer for at bringe et Kubernetes miljø op at køre med Loki + Grafana + Prometheus og TestService.

Indholdet af filerne er følgende:

- `lokigrafana.yml` - Helm `values`-fil med værdierne til at bringe et miljø baseret på Loki og Grafana op
- `prometheus.yml` - Helm `values`-fil med værdiene til at bringe et Prometheus-miljø op
- `weatherforecast.yml` - kubernetes `deployment`-fil til at bringe `TestService` op som en `POD` med tilknyttet  `Service` og `Prometheus CRD Service Monitor`
- load-test.sh - Test script til at kalde endepunkter i `TestService`

---

**Note:** Kig i filen [Howto](./Howto.md) for instrukser til at bringe et K8s-miljø op.