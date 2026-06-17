# FoodSave Landing - Contexto del proyecto

FoodSave Landing es una aplicación web ASP.NET Core MVC para presentar la
propuesta FoodSave y administrar comercios gastronómicos.

## Arquitectura actual

- Proyecto principal: `Foodsave.Web`.
- Framework: ASP.NET Core MVC sobre .NET 10.
- Persistencia: PostgreSQL con Entity Framework Core y Npgsql.
- Entidades persistidas: `Comercio`, `Titular` y `Suscripcion`.
- Migraciones: EF Core, con migración inicial PostgreSQL.
- Seed: cinco comercios de demostración cargados de forma idempotente.
- Autenticación: cookie de ASP.NET Core.

## Login de demostración

- Usuario: `comercio@foodsave.com`
- Contraseña: `123456`

La credencial continúa hardcodeada temporalmente en `AuthController`; no se
almacena en PostgreSQL. El controlador `ComerciosController` está protegido con
`[Authorize]`.

## Configuración de base de datos

La aplicación requiere PostgreSQL y resuelve la conexión con esta precedencia:

1. `ConnectionStrings__DefaultConnection`
2. `DATABASE_URL`

No existe fallback a SQLite ni se versionan credenciales reales.

## Alcance excluido

- Registro de usuarios.
- Persistencia de usuarios o roles.
- Pagos.
- Aplicaciones Expo o React Native.
- Migración de datos históricos desde el antiguo archivo SQLite.

Para desarrollo y despliegue, consultar `docs/DEPLOY_RAILWAY.md`.
