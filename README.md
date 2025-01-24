# SemistructuralProject

This project is a .NET application that uses Entity Framework Core and SQL Server. It includes functionality for managing points, cities, and countries, and provides methods for calculating distances and checking if points are within polygons.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 9.0 or later)
- [Docker](https://www.docker.com/get-started) (for DevContainer approach)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (if running directly on your machine)

## Running the Project

### Option 1: Using DevContainer

1. **Open in Visual Studio Code:**

    Open the Visual Studio Code. If you don't have the DevContainer extension installed, install it. Then create new DevContainer with newest SQLServer and .NET. When the DevContainer will be ready, proceed to the second point. 

2. **Clone the Repository inside the DevContainer using terminal:**

    ```bash
    git clone https://github.com/Dzalapino/CSharpEFCoreExample.git
    cd SemistructuralProject
    ```

3. **Build and Run the Project:**

    Once the DevContainer is set up, you can build and run the project using the integrated terminal:

    ```bash
    dotnet build
    dotnet run
    ```

    If there will be any problems with db connection, check if your connection strings are the same as the strings from .devcontainer/ files

### Option 2: Running Directly on Your Machine

1. **Clone the Repository:**

    ```bash
    git clone https://github.com/Dzalapino/CSharpEFCoreExample.git
    cd SemistructuralProject
    ```

2. **Edit Connection Strings:**

    Update the connection strings in ApplicationDBContext.cs and appsettings.json to match your local SQL Server instance.

    **ApplicationDBContext.cs:**

    ```csharp
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=YOUR_SERVER_NAME;Database=ApplicationDB;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;");
    }
    ```

    **appsettings.json:**

    ```json
    {
        "ConnectionStrings": {
            "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ApplicationDB;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;"
        }
    }
    ```

3. **Build and Run the Project:**

    Use the following commands to build and run the project:

    ```bash
    dotnet build
    dotnet run
    ```