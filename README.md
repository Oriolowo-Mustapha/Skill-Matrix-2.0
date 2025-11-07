# Skill-Matrix 2.0 API

This repository contains the backend API for Skill-Matrix 2.0, an intelligent platform designed for skill assessment and personalized improvement. The system uses AI to generate dynamic assessments for users, analyzes their performance, and then creates tailored improvement plans with recommended resources.

This project is built using **.NET 8** and strictly follows **Clean (Onion) Architecture** principles to ensure separation of concerns, maintainability, and testability.

## ✨ Core Features

* **AI-Powered Assessment Generation:** Dynamically generates test questions based on a user's selected skill and proficiency level.
* **Performance Analysis:** Analyzes assessment results to identify user weaknesses based on score trends, past performance, and proficiency.
* **Personalized Improvement Plans:** Uses AI to generate actionable feedback, focus areas, and improvement strategies.
* **Resource Recommendation:** Curates and suggests links to articles, videos, and tutorials to help users improve.
* **Role-Based Access Control:** Supports various user roles like `Learner`, `Manager`, `Team_Members`, and `Admin`.
* **Team Management:** Allows `Managers` to oversee their `TeamMembers` and track their progress.

## 💻 Tech Stack

* **Framework:** .NET 9 (ASP.NET Core Web API)
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core 8
* **Architecture:** Clean (Onion) Architecture
* **Authentication:** JWT (implied)
* **Password Hashing:** BCrypt.Net
* **Messaging (implied):** MassTransit
* **AI Service:** (e.g., OpenAI, Azure AI Services - *to be integrated*)

## 🏛️ Project Structure

The solution follows the principles of Onion Architecture, separating concerns into distinct layers:

* **`Domain`**: Contains the core business logic, entities, and enums. This layer has no dependencies on any other layer.
* **`Application`**: Contains application-level logic, including services, interfaces, DTOs, and mapping extensions. It coordinates the flow of data and orchestrates the domain logic.
* **`Infrastructure`**: Implements external concerns like the database context (`MatrixDbContext`), repositories, and services that interact with third-party APIs (like the AI service).
* **`Presentation (Skill-Matrix-2.0)`**: The API layer itself. Contains the Controllers that handle HTTP requests, a `Program.cs` for service registration, and `appsettings.json` for configuration.

## 🚀 Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [PostgreSQL](https://www.postgresql.org/download/) (or a Docker instance)
* A code editor (like VS Code or Visual Studio)

### Setup & Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/Oriolowo-Mustapha/Skill-Matrix-2.0.git](https://github.com/Oriolowo-Mustapha/Skill-Matrix-2.0.git)
    cd Skill-Matrix-2.0
    ```

2.  **Configure your settings:**
    * Open `Skill-Matrix-2.0/appsettings.Development.json`.
    * Update the `DefaultConnection` string to point to your local or hosted PostgreSQL database.
    * *(You will also add your AI service API keys here later).*

3.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

4.  **Apply database migrations:**
    This will create the database schema based on the entities.
    ```bash
    # Ensure you are in the 'Skill-Matrix-2.0' (Presentation) folder
    dotnet ef database update --project ../Infrastructure
    ```

5.  **Run the application:**
    ```bash
    dotnet run
    ```
    The API will now be running (usually on `http://localhost:5000` or `http://localhost:5123`).

## 🗺️ Roadmap

* [ ] Implement AI service client for assessment generation.
* [ ] Build the core `AssessmentService` logic for starting and submitting tests.
* [ ] Develop the performance analysis engine (based on the 4 criteria).
* [ ] Implement AI service client for generating improvement plans.
* [ ] Build out remaining API endpoints for reporting and user management.
