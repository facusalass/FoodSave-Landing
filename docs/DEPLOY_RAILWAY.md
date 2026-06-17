# Deploy de FoodSave en Railway

## Requisitos

- .NET 10 para ejecución local.
- Docker para levantar PostgreSQL localmente.
- Un proyecto Railway con un servicio PostgreSQL.

El proveedor de EF Core es
`Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2`.

## Desarrollo local

Levantar PostgreSQL:

```powershell
docker run --name foodsave-postgres `
  -e POSTGRES_DB=foodsave `
  -e POSTGRES_USER=foodsave `
  -e POSTGRES_PASSWORD=foodsave `
  -p 5432:5432 `
  -d postgres:17
```

Guardar la conexión con User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" `
  "Host=localhost;Port=5432;Database=foodsave;Username=foodsave;Password=foodsave" `
  --project Foodsave.Web
```

Restaurar, migrar y ejecutar:

```powershell
dotnet tool restore
dotnet restore
dotnet tool run dotnet-ef database update `
  --project Foodsave.Web `
  --startup-project Foodsave.Web
dotnet run --project Foodsave.Web
```

La aplicación también aplica migraciones y el seed al iniciar. Reiniciarla no
duplica los comercios de demostración.

## Variables soportadas

La aplicación usa, en este orden:

1. `ConnectionStrings__DefaultConnection`, en formato Npgsql.
2. `DATABASE_URL`, en formato `postgresql://`.

Ejemplos sin secretos reales están en `.env.example`. Ese archivo es solamente
documentación; ASP.NET Core no carga archivos `.env` automáticamente.

## Railway

1. Crear un servicio PostgreSQL dentro del proyecto Railway.
2. Crear el servicio web desde este repositorio.
3. Railway detectará el `Dockerfile` de la raíz.
4. En variables del servicio web, agregar:

```text
DATABASE_URL=${{Postgres.DATABASE_URL}}
ASPNETCORE_ENVIRONMENT=Production
```

Si el servicio PostgreSQL tiene otro nombre, reemplazar `Postgres` en la
referencia. Railway inyecta `PORT`; la aplicación escucha en
`0.0.0.0:$PORT`.

Como alternativa a `DATABASE_URL`, configurar manualmente:

```text
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require
```

No configurar ambas salvo que se quiera que
`ConnectionStrings__DefaultConnection` tenga precedencia.

## Migraciones

La migración activa es `InitialPostgreSql`. Las migraciones SQLite anteriores
fueron retiradas porque contenían tipos y anotaciones incompatibles; continúan
disponibles en el historial Git.

Para crear una migración futura:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add NombreMigracion `
  --project Foodsave.Web `
  --startup-project Foodsave.Web
```

Para aplicarla:

```powershell
dotnet tool run dotnet-ef database update `
  --project Foodsave.Web `
  --startup-project Foodsave.Web
```

En Railway, una única instancia puede aplicar migraciones al arrancar. Antes de
escalar a varias réplicas, mover esta operación a un proceso de release para
evitar carreras entre instancias.

## Verificación

1. Confirmar que el deploy inicia sin errores de conexión o migración.
2. Abrir `/Auth/Login`.
3. Ingresar con `comercio@foodsave.com` / `123456`.
4. Confirmar que `/Comercios` carga el listado desde PostgreSQL.
5. Probar detalle, alta de comercio y logout.
6. Abrir `/Comercios` sin sesión y confirmar la redirección al login.

Railway publica `DATABASE_URL` y las variables `PGHOST`, `PGPORT`, `PGUSER`,
`PGPASSWORD` y `PGDATABASE` para su servicio PostgreSQL:
https://docs.railway.com/databases/postgresql
