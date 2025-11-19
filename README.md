# File Storage & Document Management

A file storage system built with ASP.NET Core 8 and Angular 17.

## Features

- File upload, download, and preview
- User authentication with JWT
- File search and pagination
- Soft and hard delete
- Image and PDF preview

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (or SQL Server LocalDB)

## Setup

### Backend

1. Navigate to backend directory:
```bash
cd Backend
```

2. Restore packages:
```bash
dotnet restore
```

3. Update connection string in `src/FileStorage.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FileStorageDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

4. Run database migrations:
```bash
cd src/FileStorage.API
dotnet ef database update --project ../FileStorage.Infrastructure
```

5. Run the API:
```bash
dotnet run
```

The API will be available at `http://localhost:5000` or `https://localhost:7000`

### Frontend

1. Navigate to frontend directory:
```bash
cd Frontend
```

2. Install dependencies:
```bash
npm install
```

3. Update API URL in `src/environments/environment.ts` if needed:
```typescript
apiUrl: 'http://localhost:5000'
```

4. Run the development server:
```bash
npm start
```

The frontend will be available at `http://localhost:4200`

## API Endpoints

- `POST /api/auth/login` - Login
- `POST /api/files` - Upload file
- `GET /api/files` - List files (supports `pageNumber`, `pageSize`, `searchTerm` query parameters)
- `GET /api/files/{id}` - Get file metadata
- `GET /api/files/{id}/download` - Download file
- `GET /api/files/{id}/preview` - Preview file (images and PDFs)
- `DELETE /api/files/{id}` - Soft delete
- `DELETE /api/files/{id}/hard` - Hard delete (admin only)

## Database Connection

The default connection string uses SQL Server LocalDB. To use a different SQL Server instance, update the connection string in `Backend/src/FileStorage.API/appsettings.json`.
"# floragroupTask" 
