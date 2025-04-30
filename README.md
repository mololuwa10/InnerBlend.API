# Inner Blend

Inner Blend is a productivity and wellness app designed to help users manage and write down their thoughts, reminders, and personal growth, all in one place.

## 🛠️ Tech Stack

- ASP.NET Core Web API (.NET 9)
- PostgreSQL
- Entity Framework Core
- JWT Authentication
- Azure Functions (planned)
- SignalR (planned for real-time notifications)

## 🚀 Getting Started

These instructions will help you set up the project on your local machine for development.

---

## 🔧 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Azure Functions Core Tools (v4)](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Visual Studio Code](https://code.visualstudio.com/) (or any editor you prefer)
- [Postman](https://www.postman.com/) for testing the API

---

## 📦 Installation

### 1. Clone the repository

```bash
git clone https://github.com/YOUR-USERNAME/InnerBlend.API.git
cd InnerBlend.API
````

### 2. Update the database connection string

Open appsettings.Development.json and add your PostgreSQL connection string:
```bash
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=innerblend_db;Username=your_user;Password=your_password"
}
````

### 3. Apply migrations and update the database
```bash
dotnet ef database update
````

### 3. Run the application
```bash
dotnet watch run
````

## Planned Features

- Real-time reminders via SignalR or Azure Notification Hub
- Personal journaling
- Mood tracking
- Task + reminder feed with social elements
- Notification microservice (Azure Functions)

## Testing
Use Postman or any REST client to test the API routes. Make sure to attach your JWT token when required.

## ✨ Stay Tuned

More features coming soon! 
