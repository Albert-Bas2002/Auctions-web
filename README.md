# Auction Platform – Project Description

## Overview

The Auction Platform is a microservice‑based web application that enables users to create auctions, place real‑time bids, and exchange comments live. The backend is built with **ASP.NET Core Web API**, while the frontend is delivered with **React and Next.js**. Real‑time capabilities are powered by **SignalR**, ensuring sub‑second bid propagation and seamless chat experience.

## Key Technologies

* **ASP.NET Core Web API** for RESTful service endpoints
* **Microservices** communicated over REST (JSON/HTTP)
* **SignalR** hubs for bidding & chat in real time
* **React + Next.js** (TypeScript) for a modern, SEO‑friendly UI
* **SQL Server** as the primary relational data store
* **MongoDB** (like example)
* **xUnit** for unit, integration & contract testing
* **Docker & Docker Compose** for local orchestration and CI pipelines

## High‑Level Architecture

* **API Gateway**  provides a single HTTPS ingress.
* **Microservices** (Auction - Bid, Chat, Users) are hosted as independent ASP.NET Core containers.
* **Front‑End Service** runs Next.js .

## Microservices

| Service             | Responsibilities                                                                                                            |
| ------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| **Auction Service** | Create & schedule auctions, publish domain events.                                                                          |
| **Hub Service**     | Manage comment threads per auction, push new comments via SignalR. Create and Check Bids (by using Auction Service)         |
| **User Service**    | JWT‑based auth, roles-permissions.                                                                                          |

## Data Stores

* **SQL Server** per service schema (write model, transactions).

## Real‑Time Communication with SignalR

* **Bid Hub**: one logical hub per auction; clients receive the live bid stream.
* **Comment Hub**: multiplexed channel for real‑time comments.

## Business Logic & Rules

1. **Auction creation** requires a unique title, description, end timestamps, and an optional reserve price. *After creation, title,reserve and description cannot be modified.*
2. **Bidding**

   * A bid must exceed the current highest bid by at least the configured increment.
   * Bids below the reserve price are recorded but remain inactive until the reserve is met.
   * Bidding closes automatically at the auction end.
3. **Comments** are linked to the auction and streamed live via SignalR.

## Testing Strategy

* **xUnit** unit tests for domain logic,repositories and controllers.

## Deployment & DevOps

* Each microservice,Data Stores and the front‑end are packaged as Docker Container.
Use this after creating Db Containers (by docker-compose):
dump.sql in the corresponding folder contains ready data for tables and database elements

* cat dump.sql | docker exec -i UserRolePermissionContainer psql -U postgres -d UserRolePermissionDb
* cat dump.sql | docker exec -i AuctionContainer psql -U postgres -d AuctionDb
* cat dump.sql | docker exec -i AuctionChatContainer psql -U postgres -d AuctionChatDb





