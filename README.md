# Pixelz Microservices System Design

## Background

Pixelz provides AI-powered retouching for ecommerce studios. The portal allows customers to create, pay for, and manage orders.

## Architecture Overview

- **Microservices**: Each domain (Order, Payment, Invoice, Email, Transaction) is a separate service, implemented in **.NET Core** and structured using Domain Driven Design (DDD).
- **Communication**: Services communicate asynchronously via Kafka.
- **Transaction Management**: Central Transaction Service using MassTransit Outbox pattern.
- **Database**: Each service uses **MySQL** for persistence, accessed via **EF Core** ORM.
- **Deployment**: Docker Compose for orchestration.
- **Monitoring**: Grafana & DataDog.

## Services

- **Order Service**: Manages orders, search/filter by name, triggers checkout. Persists data in MySQL using EF Core.
- **Payment Service**: Handles payment processing (mocked for test), emits payment events. Persists data in MySQL using EF Core.
- **Invoice Service**: Generates invoices on successful payment. Persists data in MySQL using EF Core.
- **Email Service**: Sends notifications on successful payment (mocked). Persists data in MySQL using EF Core.
- **Production Service**: Updates internal order state (mocked). Persists data in MySQL using EF Core.
- **Transaction Service**: Ensures distributed transaction integrity via Outbox. Persists data in MySQL using EF Core.

## Data Models

- **Order**: { id, name, status, customerId, ... }
- **Payment**: { id, orderId, status, amount, ... }
- **Invoice**: { id, orderId, amount, issuedAt, ... }
- **EmailNotification**: { id, orderId, to, status, ... }

## Endpoints (REST)

- **Order Service**
  - `GET /orders?name=...` — Search/filter orders
  - `POST /orders/{id}/checkout` — Checkout order

- **Payment Service**
  - `POST /payments` — Process payment (mocked)

- **Invoice Service**
  - `POST /invoices` — Create invoice

- **Email Service**
  - `POST /emails` — Send notification (mocked)

- **Production Service**
  - `POST /production/orders` — Update order state (mocked)

## Component Interactions

1. User checks out order via Order Service.
2. Order Service emits event to Kafka.
3. Payment Service processes payment, emits success/failure event.
4. On success:
   - Invoice Service creates invoice.
   - Email Service sends notification.
   - Production Service updates internal state.
5. Transaction Service manages event consistency using Outbox.

## Assumptions & Validation

- Payment, Email, and Production services are mocked for simulation.
- Kafka ensures reliable event delivery.
- Outbox pattern prevents data loss in distributed transactions.
- Integration and load tests validate system integrity.
- Each service is organized by DDD: Domain, Application, Infrastructure, API layers.
- Data integrity is managed via transactional outbox and event-driven consistency.
- Each service manages its own MySQL database using EF Core for ORM and migrations.

## Performance Management

- Grafana dashboards for metrics.
- DataDog for distributed tracing and alerting.

## Delivery Plan

- System design & documentation: 2h
- Service scaffolding: 2h
- Kafka & Outbox integration: 2h
- Docker Compose setup: 1h
- Monitoring setup: 1h
- Testing & validation: 1h

**Total: ~9 hours**

---

## Folder Structure

```
/pixel
  /order-service
    /Domain
    /Application
    /Infrastructure
    /Api
  /payment-service
    /Domain
    /Application
    /Infrastructure
    /Api
  /invoice-service
    /Domain
    /Application
    /Infrastructure
    /Api
  /email-service
    /Domain
    /Application
    /Infrastructure
    /Api
  /production-service
    /Domain
    /Application
    /Infrastructure
    /Api
  /transaction-service
    /Domain
    /Application
    /Infrastructure
    /Api
  docker-compose.yml
  README.md
```

## How to Run

1. Clone repo.
2. Ensure Docker is installed.
3. `docker-compose up --build`
4. Each service will automatically connect to its own MySQL database using EF Core migrations.
5. Access Grafana/DataDog dashboards for monitoring.

---

## Notes

- Designed for scalability (10-20 engineers).
- DDD boundaries per service.
- Event-driven, resilient, observable.
- .NET Core solution, MassTransit for Outbox, Kafka for async communication.
- **Each service uses MySQL with EF Core for data persistence.**
- Mocked Payment, Email, and Production APIs for demo/testing.
