\# MicroservicesSample



A \*\*practice project\*\* built with ASP.NET Core to learn and implement a simple microservices architecture and DevOps workflow.



\## 🛠️ Technologies



\* ASP.NET Core

\* C#

\* Entity Framework Core

\* SQL Server

\* Redis

\* RabbitMQ

\* YARP API Gateway

\* Docker \& Docker Compose

\* GitHub Actions

\* GitHub Container Registry (GHCR)

\* Self-hosted GitHub Actions Runner

\* Ubuntu



\## 🏗️ Architecture



The project consists of the following services:



\* \*\*ApiGateway\*\* — Routes requests to the appropriate microservice using YARP.

\* \*\*ProductService\*\* — Manages products and product-related data.

\* \*\*OrderService\*\* — Handles orders and communicates with ProductService.

\* \*\*NotificationService\*\* — Consumes messages from RabbitMQ.



Supporting infrastructure:



\* SQL Server

\* Redis

\* RabbitMQ



\## 🚀 Getting Started



\### Prerequisites



Make sure the following are installed:



\* .NET SDK 10

\* Docker

\* Docker Compose

\* SQL Server

\* Git



\### Configuration



For local development, configure the required values in the appropriate `appsettings.Development.json` files or environment variables.



For Docker, create a `.env` file next to `compose.prod.yml` and provide the required variables:



```env

SQL\_PASSWORD=your\_sql\_password



RABBITMQ\_HOST=rabbitmq

RABBITMQ\_PORT=5672

RABBITMQ\_USERNAME=guest

RABBITMQ\_PASSWORD=guest



REDIS\_HOST=redis

REDIS\_PORT=6379



PRODUCT\_SERVICE\_HOST=productservice

PRODUCT\_SERVICE\_PORT=8080

```



\### Database



For local development, create the databases using EF Core migrations.



Run the following command for each service that has its own database (`ProductService` and `OrderService`):



```bash

dotnet ef database update

```



\## 🐳 Docker



To start the Docker environment:



```bash

docker compose -f compose.prod.yml up -d

```



Check the running containers:



```bash

docker compose -f compose.prod.yml ps

```



View live logs:



```bash

docker compose -f compose.prod.yml logs -f

```



To view logs for a specific service:



```bash

docker compose -f compose.prod.yml logs -f orderservice

```



\## 🧪 Testing



The APIs can be tested using \*\*Swagger\*\* or \*\*Postman\*\*.



Example:



```http

DELETE /api/orders/{id}

```



\## ⚙️ CI/CD



The project uses \*\*GitHub Actions\*\* to automate the CI/CD workflow.



On every push to the `main` branch:



1\. The application is built.

2\. Docker images are created.

3\. Images are pushed to GitHub Container Registry.

4\. The self-hosted Ubuntu runner pulls the latest images.

5\. Docker Compose deploys the updated services.



\## 📌 Purpose



This project is primarily for learning and practicing:



\* Microservices architecture

\* Inter-service communication

\* RabbitMQ

\* Redis caching

\* Docker and Docker Compose

\* CI/CD with GitHub Actions

\* GitHub Container Registry

\* Self-hosted runners

\* Deployment on Ubuntu



This is a \*\*learning project\*\* and is not intended to be production-ready.



