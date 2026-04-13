# Nalbur - Small Business Management System

A production-oriented .NET 8 WPF application for managing stock, customers, sales, and installments.

## Key Features
- **Dashboard**: Overview of low stock and overdue installments.
- **Product Management**: CRUD for products with stock tracking.
- **Customer Management**: Detailed customer records.
- **Sales Transaction**: POS-style interface supporting Cash, Card, and Installment sales.
- **Installment Tracking**: Reminder system for upcoming and past-due payments.
- **Data Persistence**: EF Core with SQL Server.

## How to Run

### Prerequisites
- **.NET 8 SDK**
- **SQL Server LocalDB** (Default connection: `(localdb)\MSSQLLocalDB`)

### Steps
1. Clone the repository.
2. Open a terminal in the solution root.
3. Run the following command to ensure the database and seed data are initialized:
   ```bash
   dotnet run --project Nalbur.Wpf
   ```
   *Note: On first run, the `DataSeeder` will automatically apply migrations and create sample data.*

### Default Credentials
- **Username**: `admin`
- **Password**: `admin`
- *Note: Login screen is not fully enforced in this MVP, but base entities exist for it.*

## Architecture
- **Layered Architecture**: Separation of Domain, Infrastructure, and UI.
- **MVVM Pattern**: Using `CommunityToolkit.Mvvm`.
- **Dependency Injection**: Microsoft Hosting for service management.
- **Modern UI**: Styled with `MaterialDesignInXamlToolkit`.

## Future Improvements
- Barcode scanner integration (Product lookup).
- Receipt printer support (using `System.Printing`).
- Advanced reporting with charts (OxyPlot or LiveCharts).
