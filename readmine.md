## Migration
```shaderlab
dotnet ef migrations add InitialCreatPg --startup-project BlogBank.Api --project BlogBank.Infrastructure
dotnet ef database update --startup-project BlogBank.Api --project BlogBank.Infrastructure 
```