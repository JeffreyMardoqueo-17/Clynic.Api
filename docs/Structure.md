````
📁 Clynic
├── 📁 src
│   ├── 📁 Clynic.Api
│   │   ├── 📁 Controllers
│   │   │   └── ClinicsController.cs
│   │   │
│   │   ├── 📁 Middlewares
│   │   │   └── ExceptionMiddleware.cs
│   │   │
│   │   ├── 📁 Filters
│   │   │   └── ValidationFilter.cs
│   │   │
│   │   ├── 📁 Configurations
│   │   │   └── SwaggerConfiguration.cs
│   │   │
│   │   ├── 📁 Extensions
│   │   │   └── ServiceCollectionExtensions.cs
│   │   │
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Program.cs
│   │   ├── Clynic.Api.csproj
│   │   └── Dockerfile
│   │
│   ├── 📁 Clynic.Application
│   │   ├── 📁 DTOs
│   │   │   └── Clinics
│   │   │       ├── CreateClinicDto.cs
│   │   │       ├── UpdateClinicDto.cs
│   │   │       └── ClinicResponseDto.cs
│   │   │
│   │   ├── 📁 Interfaces
│   │   │   ├── Repositories
│   │   │   │   └── IClinicRepository.cs
│   │   │   │
│   │   │   └── Services
│   │   │       └── IClinicService.cs
│   │   │
│   │   ├── 📁 Services
│   │   │   └── ClinicService.cs
│   │   │
│   │   ├── 📁 Mappings
│   │   │   └── ClinicMappingProfile.cs
│   │   │
│   │   └── Clynic.Application.csproj
│   │
│   ├── 📁 Clynic.Domain
│   │   ├── 📁 Entities
│   │   │   └── Clinic.cs
│   │   │
│   │   ├── 📁 ValueObjects
│   │   │   └── Address.cs
│   │   │
│   │   ├── 📁 Enums
│   │   │   └── ClinicStatus.cs
│   │   │
│   │   └── Clynic.Domain.csproj
│   │
│   ├── 📁 Clynic.Infrastructure
│   │   ├── 📁 Data
│   │   │   ├── AppDbContext.cs
│   │   │   │
│   │   │   ├── 📁 Configurations
│   │   │   │   └── ClinicConfiguration.cs
│   │   │   │
│   │   │   └── 📁 Migrations
│   │   │       └── (EF Core migrations files)
│   │   │
│   │   ├── 📁 Repositories
│   │   │   └── ClinicRepository.cs
│   │   │
│   │   ├── 📁 Persistence
│   │   │   └── DatabaseSeeder.cs
│   │   │
│   │   ├── 📁 DependencyInjection
│   │   │   └── InfrastructureServiceRegistration.cs
│   │   │
│   │   └── Clynic.Infrastructure.csproj
│
├── 📁 tests
│   ├── 📁 Clynic.UnitTests
│   │   └── ClinicServiceTests.cs
│   │
│   └── 📁 Clynic.IntegrationTests
│       └── ClinicsControllerTests.cs
│
├── 📁 docker
│   ├── docker-compose.yml
│   │
│   └── 📁 sqlserver
│       ├── init.sql
│       └── seed-data.sql
│
├── 📁 docs
│   └── architecture-diagram.md
│
├── 📁 scripts
│   ├── migrate-db.sh
│   └── seed-db.sh
│
├── .gitignore
├── README.md
└── Clynic.sln

````