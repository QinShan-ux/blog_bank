## Migration
```shaderlab
dotnet ef migrations add InitialCreatePg `
  --context AppDbContext `
  --startup-project src/BlogBank.Api `
  --project src/BlogBank.Infrastructure
dotnet ef database update --startup-project BlogBank.Api --project BlogBank.Infrastructure 
```

## 容器编排
```shell
# 创建rabbitmq 容器
docker compose -f compose_rabbitmq.yml up -d

```