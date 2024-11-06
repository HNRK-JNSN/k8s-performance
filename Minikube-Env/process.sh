#!/bin/bash

# https://medium.com/@muppedaanvesh/a-hands-on-guide-to-kubernetes-logging-using-grafana-loki-%EF%B8%8F-b8d37ea4de13

# Install Helm on Windows
winget install Helm.Helm

# Install Helm on MacOS 
brew install helm

# Add the Grafana & Prometheus Helm repository
helm repo add grafana https://grafana.github.io/helm-charts

# Update the Helm repositories
helm repo update

# Search repo for Loki charts
helm search repo loki

# Show the values of the Loki chart
helm show values grafana/loki-stack > lokigrafana.yaml

# Enable grafana
# add service to grafana rule:
#   service:
#     type: NodePort

# Deploy the Loki stack with Custom values

helm upgrade --install --values lokigrafana.yaml loki grafana/loki-stack -n monitoring --create-namespace

# Check the status of the pods
kubectl get pods -n monitoring

# Find the NodePort of the Grafana service
kubectl get svc loki-grafana -n monitoring  -o jsonpath="{.spec.ports[0].nodePort}"

# Get the password for the Grafana admin user
kubectl get secret loki-grafana -n monitoring -o jsonpath="{.data.admin-user}" | base64 --decode

# Get the password for the Grafana admin user
kubectl get secret loki-grafana -n monitoring -o jsonpath="{.data.admin-password}" | base64 --decode

# ------
# Install Prometheus Helm Chart
# ------

helm repo add prometheus-community https://prometheus-community.github.io/helm-charts

helm show values prometheus-community/kube-prometheus-stack > prometheus.yaml

helm install kube-prometheus-stack prometheus-community/kube-prometheus-stack --namespace monitoring --values prometheus.yaml

# -----------------------------------------------------
# Access Grafana
# -----------------------------------------------------

# List Monitoring Services
kubectl get services -n monitoring

### Tunnel into the Grafana pod (blocking terminal!)
minikube service loki-grafana -n monitoring --url

# -----------------------------------------------------
# Access Prometheus
# -----------------------------------------------------

### Tunnel into the Grafana pod (blocking terminal!)
minikube service kube-prometheus-stack-prometheus -n monitoring --url

# -----------------------------------------------------
# Weatherforecast Service 
# -----------------------------------------------------

# Deploy Weatherforecast Pods
kubectl apply -f weatherforecast-svc.yaml

# Redeploy Weatherforecast
kubectl rollout restart deployment weatherforecast

# Expose Weatherforecast service
kubectl expose deployment weatherforecast --type=NodePort --port=8080

# List Services in default namespace
kubectl get services

### Tunnel into the Grafana pod (blocking terminal!)
minikube service weatherforecast -n default --url

# Note: metrics on weatherforecast is availabale from http://WeathforecastURL/metrics

# -----------------------------------------------------
