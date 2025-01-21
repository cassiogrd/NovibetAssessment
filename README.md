## 📋 Description
The Novibet project is a web application that provides functionalities for managing IP information and generating country-based reports. The system integrates with Redis for caching, performs database queries using Entity Framework and Dapper, and makes external API calls to enrich data.

## 🚀 Features
- **IP Information Lookup:**
  - Search in cache (Redis).
  - Query the database.
  - External API fallback for data enrichment.
- **Scheduled IP Updates:**
  - Automated jobs with Quartz.NET.
- **Country-based Reports:**
  - SQL queries using Dapper for custom reports.
- **Unknown Country Handling:**
  - Ensures data integrity by mapping unknown countries appropriately.

## 🛠️ Technologies Used
- **.NET 8:** Main backend framework.
- **Entity Framework:** ORM for database management.
- **Dapper:** Lightweight SQL querying for reports.
- **Redis:** Caching for temporary data storage.
- **Quartz.NET:** Task scheduler for periodic updates.
- **xUnit:** Unit testing framework.
- **MoQ:** Mocking dependencies for testing.
- **FluentAssertions:** Fluent validation for tests.

## 🗂️ Project Structure
```plaintext
/Novibet
├── Controllers         # Application controllers (API)
├── Interfaces          # Contract definitions (interfaces)
├── Models              # Data model classes
├── Repositories        # Data access layer
├── Services            # Business logic and cache/API integration
├── Jobs                # Automated tasks (Quartz)
├── Tests               # Unit tests using xUnit
└── Program.cs          # Application startup configuration
```

## 🔄 Program Workflow

The application follows a structured workflow to manage IP information and generate reports. Below is a high-level overview:

### 1️⃣ IP Information Lookup
1. **Request Handling**: 
   - A user makes an API call to `/api/ipinfo/{ip}` with an IP address.
   - The controller (`IpInfoController`) validates the input and forwards the request to the `IpInfoService`.

2. **Cache Check**:
   - The `IpInfoService` checks if the data for the given IP is available in Redis.
   - If found, the data is returned immediately.

3. **Database Query**:
   - If not found in the cache, the service queries the database using `IpInfoRepository`.
   - If data is found, it is returned and also cached in Redis.

4. **External API Call**:
   - If not found in the database, the service calls an external API (`ip2c.org`) to fetch the data.
   - The country information is validated:
     - If the country exists in the database, the IP is associated with it.
     - If not, it is linked to a default "Unknown Country."

5. **Caching and Response**:
   - The data is cached in Redis for future requests.
   - The response is returned to the client.

---

### 2️⃣ Automated IP Updates
1. **Job Scheduling**:
   - The `IpUpdateJob` is scheduled using `Quartz.NET` to run hourly.

2. **Batch Processing**:
   - The job fetches IPs from the database in batches (e.g., 100 at a time).
   - For each IP, the external API is queried to update country information.

3. **Data Validation**:
   - If country information changes, the database is updated.
   - The cache is invalidated and refreshed with updated data.

4. **Logging**:
   - Every operation is logged using `Serilog`.

---

### 3️⃣ Country-Based Reports
1. **Report Request**:
   - A user makes an API call to `/api/reports/addresses-per-country` with optional filters for specific countries.

2. **Raw SQL Query**:
   - The `ReportRepository` executes a raw SQL query using `Dapper` to retrieve data:
     - Number of IP addresses per country.
     - Last updated timestamp for each country.

3. **Response**:
   - The formatted data is returned to the client.

---

### 🧪 Testing Workflow
1. **Unit Tests**:
   - The `IpInfoServiceTests` ensure the service handles various scenarios, including:
     - Cache hits/misses.
     - Database lookups.
     - Unknown country handling.

2. **Mocked Dependencies**:
   - All external dependencies, such as Redis and the repository, are mocked using Moq.
