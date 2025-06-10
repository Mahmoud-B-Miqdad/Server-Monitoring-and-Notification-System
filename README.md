# 🚀 Server Monitoring and Notification System

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Modern%20Backend-brightgreen?logo=csharp)
![MongoDB](https://img.shields.io/badge/Database-MongoDB-green?logo=mongodb)
![RabbitMQ](https://img.shields.io/badge/Messaging-RabbitMQ-ff6600?logo=rabbitmq)
![SignalR](https://img.shields.io/badge/Realtime-SignalR-blue?logo=signalr)
![Docker Support](https://img.shields.io/badge/Docker-Supported-2496ED?logo=docker)
![Live Alerts](https://img.shields.io/badge/Live_Notifications-Enabled-yellow?logo=livechat)
![Status](https://img.shields.io/badge/System-Real--Time-critical?logo=dotnet)

> A robust and extensible **C# .NET system** for real-time **server monitoring, anomaly detection**, and **live notifications** using **RabbitMQ**, **MongoDB**, and **SignalR**.  
> Built as part of my backend training at **Foothill Technology Solutions, LLC**.

---

## 📦 Features

- ✅ Collects memory, CPU usage, and available memory at configurable intervals
- 📤 Publishes metrics to a RabbitMQ topic queue
- 🧠 Detects anomalies & high usage spikes using smart thresholds
- 🛢 Persists server stats into MongoDB
- 📡 Sends real-time alerts using SignalR
- 🖥 Console-based live event listener
- 🐳 Docker-ready & easily orchestrated with Docker Compose
- ♻️ Modular design using clean abstractions

---

## 🧰 Technologies Used

| Layer        | Tech Stack |
|--------------|------------|
| Language     | C# (.NET 8) |
| Messaging    | RabbitMQ (via Abstractions) |
| Database     | MongoDB |
| Realtime     | SignalR |
| Config       | `appsettings.json` + Env Vars |
| Containers   | Docker, Docker Compose |
| Architecture | SOLID, Clean Code, Interfaces, Separation of Concerns |

---

## 📁 Project Structure

```bash
Server-Monitoring-and-Notification-System/
├── ServerMonitoringSystem.Shared/         # Shared Classes
├── MessagingLibrary/                      # RabbitMQ abstraction layer
├── ServerMonitoringSystem.Infrastructure/ # MongoDB abstraction layer
├── ServerMonitoringSystem.MessagePublisher/   # Collects & publishes metrics
├── ServerMonitoringSystem.MessageProcessor/   # Stores to DB & detects anomalies and Sends alerts via SignalR    
├── ServerMonitoringSystem.ConsumerClient/     # Console listener for SignalR
├── ServerMonitoringSystem.SignalRHub/         # Send Alert for all listeners
├── docker-compose.yml                    # Full system orchestration
└── README.md                             # Project documentation
```
---

## ⚙️ How It Works

---

### 1. 🧪 Statistics Publisher Service

![Metrics](https://img.shields.io/badge/System%20Metrics-Memory%20%7C%20CPU-blue?style=flat-square&logo=monitor)

- Collects system metrics every **`SamplingIntervalSeconds`**
- Publishes data to RabbitMQ topic:  
  **`ServerStatistics.<ServerIdentifier>`**

---

### 2. 🧠 Anomaly Detection & Storage Service

![MongoDB](https://img.shields.io/badge/Storage-MongoDB-green?style=flat-square&logo=mongodb)
![Anomaly Detection](https://img.shields.io/badge/Detection-Anomalies%20%7C%20High%20Usage-red?style=flat-square&logo=ai)

- Consumes messages from **RabbitMQ**
- Stores server statistics into **MongoDB**
- Detects anomalies based on:
  - 🧠 **Sudden spikes** in memory or CPU usage
  - 🚨 **High usage threshold** breaches
- Sends alerts via the **SignalR hub**

---

### 3. 📡 SignalR Console Consumer

![SignalR](https://img.shields.io/badge/Realtime-Console%20Alerts-blue?style=flat-square&logo=dotnet)

- Connects to the **SignalR hub**
- Displays **real-time alerts** directly in the terminal

---

## 🐳 Run with Docker

![Docker](https://img.shields.io/badge/Docker-Compose%20Support-2496ED?style=flat-square&logo=docker)

To build and run all services using Docker Compose:

```bash
# Build & Run all services
docker-compose up --build
```

✅ All services (Publisher, Consumer, SignalR Hub, MongoDB, RabbitMQ) will be up and integrated automatically
