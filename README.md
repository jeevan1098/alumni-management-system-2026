# Alumni Management System

A comprehensive web application for managing alumni data, profiles, and interactions built with ASP.NET Core 9.0.

## 🎯 Features

- **User Authentication & Authorization**: Secure login system with role-based access control
- **Alumni Registry**: Centralized database of all alumni with bulk import capabilities
- **Alumni Profiles**: Detailed profiles including education, employment, and contact information
- **Degree Management**: Track alumni degrees and academic achievements
- **Employment Tracking**: Monitor alumni career progression and current employment
- **Internship Records**: Maintain records of alumni internships
- **Messaging System**: Internal messaging between alumni and administrators
- **Organization Management**: Track alumni involvement in organizations
- **Bulk Import**: Import alumni data from CSV or Excel files
- **JAG ID Validation**: Enforced format validation for university JAG IDs

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0 (MVC + Razor Pages)
- **Database**: SQL Server (LocalDB for development)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity with custom user model
- **UI Framework**: Bootstrap 5.3.3
- **Excel Processing**: EPPlus 7.5.2 (NonCommercial)

## 👥 User Roles

The system supports three distinct roles:

1. **Admin**: Full system access, user management, and configuration
2. **Alumni**: Alumni users with access to their profiles and messaging
3. **Staff**: Staff members with viewing and reporting capabilities

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Alumni_Management_System
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (if needed)

   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Alumni_Management_System;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**

   Open your browser and navigate to: `https://localhost:7246`

## 🔑 Default Login Credentials

The system comes with pre-seeded test accounts:

### Admin Account
- **Username**: admin
- **Password**: Admin@123
- **JAG ID**: J0000001

### Staff Account
- **Username**: staff
- **Password**: Staff@123
- **JAG ID**: J0000002

### Alumni Accounts
- **Username**: alumni
- **Password**: Alumni@123
- **JAG ID**: J0010002

Additional alumni accounts: jane.smith@email.com, michael.johnson@email.com, sarah.williams@email.com, david.brown@email.com (all with Password1!)

## 📋 JAG ID Format

All JAG IDs in the system must follow this format:
- **Pattern**: `J00` followed by numbers only
- **Regex**: `^J00\d+$`
- **Valid Examples**: J0012345, J00001, J0099999
- **Invalid Examples**: J12345, A0012345, J00ABC45

## 📥 Bulk Import

### Importing Alumni Registry Data

1. Navigate to **Alumni Registries** → **Bulk Import**
2. Prepare your CSV or Excel file with the following format:

```csv
JAG ID,First Name,Last Name,Graduation Year,Degree Program,Email On Record
J0012345,John,Doe,2020,Computer Science,john.doe@example.com
J0012346,Jane,Smith,2021,Business Administration,jane.smith@example.com
```

3. Upload the file and review the import results
4. The system will validate JAG IDs and detect duplicates automatically

**Sample File**: A sample CSV file is available at `SampleData/AlumniRegistry_Sample.csv`

## 🗄️ Database Schema

### Core Entities

- **AppUser**: Extended Identity user with JAG ID
- **Alumni**: Detailed alumni profiles
- **AlumniRegistry**: Central registry of all alumni
- **AlumniDegree**: Academic degrees earned
- **AlumniEmployment**: Employment history
- **AlumniInternship**: Internship records
- **Message**: Internal messaging
- **AlumniOrganization**: Organization memberships
- **DegreeProgram**: Available degree programs
- **Employer**: Employer information
- **OrganizationType**: Types of organizations

## 🔧 Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Reset database:
```bash
dotnet ef database drop
dotnet ef database update
```

### Running Tests

```bash
dotnet test
```

## 📝 Sample Data

The application includes comprehensive seed data:
- 8 user accounts (1 Admin, 1 Staff, 5 Alumni, 1 unassigned)
- 7 degree programs
- 7 employers
- 5 organization types
- 7 alumni registry entries

All seed data is automatically created on first run.

## 🔐 Security Features

- Password requirements enforced by ASP.NET Core Identity
- Role-based authorization
- Anti-forgery token validation
- Secure password hashing
- Email confirmation support (configurable)

## 📖 Additional Documentation

- **Implementation Notes**: See `IMPLEMENTATION_NOTES.md` for detailed technical documentation
- **Change Log**: Recent updates and feature additions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 📧 Support

For issues or questions, please contact the system administrator or create an issue in the repository.

## 🔄 Recent Updates

- ✅ JAG ID validation with regex pattern enforcement
- ✅ Three-role system (Admin, Alumni, Staff)
- ✅ Bulk import functionality for CSV and Excel files
- ✅ Comprehensive seed data for testing
- ✅ Enhanced UI with validation helpers
