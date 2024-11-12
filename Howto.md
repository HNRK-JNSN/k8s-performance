# Quickguide: Minikube cluster med Loki, Grafana og Prometheus

Denne guide indeholder information omkring hvordan du opretter et K8s-miljø til et `TestService` og tilhørende monitorerings værktøjer i form af: Loki, Grafana og Prometheus.

---

## Byg docker image

1) Fra en terminal gå ind i folderen `Staging` byg en container-image med kommandoen:
  
   ``` bash
    $ docker compose build
   ```
  
   > Ønskes image't placeret i dit eget DockerHub registry skal kontonavnet redigeres i `docker-compose.yml`-filen.
  
---

## Opdater lokale Helm repo

Fra en terminal kør følgende `Helm`-kommandoer:

```bash
 $ helm repo add grafana https://grafana.github.io/helm-charts
 $ helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
 $ helm repo update 
```

---

## Tilføj Grafana og Loki til miljøet

Fra folderen `Minikube-Env` kør kommandoen:

```bash
 $ helm upgrade --install --values lokigrafana.yaml loki grafana/loki-stack -n monitoring --create-namespace
```

---

## Tilføj Prometheus til miljøet

Fra folderen `Minikube-Env` kør kommandoen:

```bash
  $ helm install kube-prometheus-stack prometheus-community/kube-prometheus-stack --namespace monitoring --values prometheus.yaml --create-namespace
```

## Tjek installationen

Tjek at der tilføjet de nødvendige Pods til dit `monitoring`-namespace i dit cluster:

```bash
$ kubectl get services -n monitoring
```

---

## Tilføj TestService til clustered

Tilføj nu en container af imaget som blev bygget tidligere. Kig filen igennem for nødvendige tilpasning, som f.eks. image-navnet. Kør herefter:

```
 $ kubectl apply -f weatherforecast-svc.yaml
```

---

## Opret Tunnel ind i TestService

For at tilgå test service'en udefra skal der oprettes en tunnel ind til den service som blev oprettet herover. Kør kommandoen:

```bash
 $ minikube service weatherforecast-service -n default --url
```

Med en browser, test at du kan tilgå den viste URL. Kig evt. i koden for servicen for at se de resterende endepunkter som servicen eksponere.

---

## Opret Tunnel ind i Grafana

For at tilgå Grafana skal der også laves en tunnel hertil:

```bash
 $ minikube service loki-grafana -n monitoring --url
```

Med en browser, test at du kan tilgå den viste URL. Kig evt. i koden for servicen for at se de resterende endepunkter som servicen eksponere.

---

## Opret en Redis server in din klynge

1) For at tilføje en Redis cacheserver til løsningen skal du installer den med følgende Helm Chart:
  
   ```bash
   $ helm install testserver -n caching --create-namespace \
         oci://registry-1.docker.io/bitnamicharts/redis
   ```

2) Følg instrukserne fra installationsscriptet:
   - gem URL'en for at tilgå din `Redis`-server.
   - Kør kommandoen for at finde dit password og gem det et sikkert sted:
  
     ```bash
      $ export REDIS_PASSWORD=$(kubectl get secret --namespace default my-release-redis \
          -o jsonpath="{.data.redis-password}" | base64 -d)
      $ echo $REDIS_PASSWORD
     ```

3) Husk at tilpasse `weatherforecast-svc.yaml` en **connection-string** under miljøvariablerne (bemærk underscore anvendt som separator i navnet):

   ```yaml
      env:
      - name: RedisConnectionString
        value: "my-release-redis-master.default.svc.cluster.local:6379"
   ```

4) Tilføj et k8s `Secret` til toppen af dit deployment script:
  
   **Bemærk:** Strengene i `data`-gruppen herunder, skal enkodes med **base64**, det kan du gøre fra en terminal-kommandoen:
    &nbsp;
    `echo -n MIN_STRENG | base64`
    &nbsp;
  
   ```yaml
    #########################################################
    # Secrets
    #########################################################
    apiVersion: v1
    kind: Secret
    metadata:
      name: redis-credentials
    type: Opaque
    data:
      password: Y284ZU1wbFdqRw==

    ---
   ```

5) Genstart dit `weatherforcast-svc` deployment med:
  
   ```bash
    $ kubetcl apply -f weatherforecast-svc.yaml
    $ kubectl rollout restart deployment weatherforecast-deployment
   ```

---
