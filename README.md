# Loan Management System

A full-stack Loan Management System built with .NET Core (C#) backend and Angular frontend.

## Overview

This application provides a RESTful API for managing loan applications and a simple Angular frontend to display loan information. The system includes:

- **Backend**: .NET 6.0 Web API with Entity Framework Core and SQL Server
- **Frontend**: Angular 19 with Material Design
- **Database**: SQL Server with seed data
- **Testing**: Unit and integration tests using xUnit
- **DevOps**: Docker and Docker Compose for containerization

## Features

### Backend API Endpoints

- `POST /loans` - Create a new loan
- `GET /loans` - List all loans
- `GET /loans/{id}` - Retrieve loan details by ID
- `POST /loans/{id}/payment` - Make a payment on a loan

### Frontend

- Display list of loans in a table format
- Real-time data fetching from backend API
- Loading and error states
- Responsive design

## Setup Instructions

### Prerequisites

- .NET 6.0 SDK
- Node.js (v18 or higher)
- Docker and Docker Compose (optional, for containerized deployment)
- SQL Server (if not using Docker)

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/src
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update the connection string in `Fundo.Applications.WebApi/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=LoanManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
}
```

4. Run the API:
```bash
cd Fundo.Applications.WebApi
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the Angular development server:
```bash
npm start
```

The frontend will be available at `http://localhost:4200`.

### Running with Docker Compose

1. From the root directory, run:
```bash
docker-compose up
```

This will start both the SQL Server database and the .NET API in containers.

### Running Tests

#### Backend Tests

1. Navigate to the backend test directory:
```bash
cd backend/src/Fundo.Services.Tests
```

2. Run tests:
```bash
dotnet test
```

#### Frontend Tests

```bash
cd frontend
npm test
```

## API Documentation

Once the backend is running, access the Swagger UI at `http://localhost:5000/swagger` for interactive API documentation.

### Loan Model

```json
{
  "id": 1,
  "amount": 25000.00,
  "currentBalance": 18750.00,
  "applicantName": "John Doe",
  "status": "active",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

### Create Loan Request

```json
{
  "amount": 15000.00,
  "applicantName": "Jane Smith"
}
```

### Payment Request

```json
{
  "amount": 5000.00
}
```

## Implementation Approach

### Architecture

- **Clean Architecture**: Separation of concerns with Models, DTOs, and Controllers
- **Repository Pattern**: DbContext for data access
- **Dependency Injection**: Services injected through constructor
- **DTOs**: Separate data transfer objects for API contracts

### Key Decisions

1. **Entity Framework Core**: Chosen for its robust ORM capabilities and SQL Server integration
2. **xUnit**: Selected for testing due to its simplicity and wide adoption in .NET ecosystem
3. **Angular Material**: Used for UI components to provide a consistent, professional look
4. **Swagger**: Included for API documentation and testing
5. **Docker**: Containerization for easy deployment and environment consistency

### Challenges Faced

1. **Initial Project Structure**: Had to reorganize the existing structure to follow clean architecture principles
2. **Database Seeding**: Implemented a custom DbInitializer to populate seed data on startup
3. **CORS Configuration**: Added CORS policy to allow frontend-backend communication
4. **Angular Service Integration**: Created a service layer to handle HTTP requests with proper error handling

### Features Implemented

✅ All required API endpoints (POST /loans, GET /loans, GET /loans/{id}, POST /loans/{id}/payment)
✅ Entity Framework Core with SQL Server
✅ Seed data for initial loans
✅ Unit tests for API endpoints
✅ Integration tests for API with business logic validation
✅ Docker and Docker Compose configuration with healthcheck
✅ Angular frontend with Material Design table
✅ Real-time data fetching from API
✅ Loading and error states in frontend
✅ Swagger API documentation
✅ GitHub Actions CI/CD pipeline for automated testing

### Potential Improvements

Given more time, the following improvements could be made:

1. **Authentication**: Implement JWT-based authentication and authorization
2. **Validation**: Add more comprehensive input validation with FluentValidation
3. **Logging**: Implement structured logging with Serilog
4. **Pagination**: Add pagination support for the loans list endpoint
5. **Filtering and Sorting**: Add filtering and sorting capabilities to the API
6. **Frontend Forms**: Add forms for creating loans and making payments
7. **Error Handling**: Implement global error handling middleware
8. **API Versioning**: Implement API versioning for future compatibility
9. **Unit Tests for Frontend**: Add unit tests for Angular components and services

## Project Structure

```
take-home-test/
├── backend/
│   └── src/
│       ├── Fundo.Applications.WebApi/
│       │   ├── Controllers/
│       │   ├── Data/
│       │   ├── DTOs/
│       │   ├── Models/
│       │   └── Program.cs
│       └── Fundo.Services.Tests/
│           ├── Integration/
│           └── Unit/
├── frontend/
│   └── src/
│       └── app/
│           ├── services/
│           └── ...
├── docker-compose.yml
└── README.md
```

## Contact

For questions or issues, please refer to the original take-home test instructions.