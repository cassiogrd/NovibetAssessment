📋 Description
The Novibet project is a web application that provides functionalities for managing IP information and generating country-based reports. The system integrates with Redis for caching, performs database queries using Entity Framework and Dapper, and makes external API calls to enrich data.

🚀 Features
IP Information Lookup:
  Search in cache (Redis).
  Query the database.
  External API fallback for data enrichment.
  
Scheduled IP Updates:
  Automated jobs with Quartz.NET.
  
Country-based Reports:
  SQL queries using Dapper for custom reports.
  
Unknown Country Handling:
  Ensures data integrity by mapping unknown countries appropriately.


🛠️ Technologies Used
.NET 8: Main backend framework.
Entity Framework: ORM for database management.
Dapper: Lightweight SQL querying for reports.
Redis: Caching for temporary data storage.
Quartz.NET: Task scheduler for periodic updates.
xUnit: Unit testing framework.
MoQ: Mocking dependencies for testing.
FluentAssertions: Fluent validation for tests.


## 🗂️ **Project Structure**
/Novibet
├── Controllers         # Application controllers (API)
├── Interfaces          # Contract definitions (interfaces)
├── Models              # Data model classes
├── Repositories        # Data access layer
├── Services            # Business logic and cache/API integration
├── Jobs                # Automated tasks (Quartz)
├── Tests               # Unit tests using xUnit
└── Program.cs          # Application startup configuration
