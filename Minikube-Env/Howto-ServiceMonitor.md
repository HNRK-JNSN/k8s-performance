# How to create a service monitor deployment for Prometheus

## What are CRDs?

Before we dive into the issue at hand, let’s quickly review the basics. Custom Resource Definitions (CRDs) are a Kubernetes feature that allows you to extend the Kubernetes API with your own custom resources. These custom resources enable you to define and manage applications or services specific to your needs. CRDs play a crucial role in extending Kubernetes capabilities beyond its built-in resource types.

- [Creating a Service Monitor in Kubernetes](https://apgapg.medium.com/creating-a-service-monitor-in-kubernetes-c0941a63c227)

## Get details of Prometheus service

```bash
kubectl describe service weatherforecast --namespace default
```

---

## Create ServiceMonitor yaml

```yaml

apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: weatherforecast-prometheus
  namespace: prometheus  
  labels:
    serviceMonitorSelector: prometheus
spec:
  endpoints:
  - interval: 20s
    targetPort: 9090    
    path: /metrics  
  namespaceSelector:
    matchNames:
    - prometheus
  selector:
    matchLabels:
      app: "true"

```
