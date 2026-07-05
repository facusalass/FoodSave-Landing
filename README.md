# FoodSave Landing — Documentación Técnica

## 1. Descripción del proyecto

FoodSave Landing es una aplicación web desarrollada en **ASP.NET Core MVC (.NET 10)** que funciona como portal institucional para el ecosistema FoodSave. Permite a comercios gastronómicos solicitar su incorporación a la plataforma, y a los administradores gestionar comercios, suscripciones, pagos y estadísticas desde un panel interno.

La aplicación se integra con el backend principal de FoodSave (Node.js + Supabase) mediante una API REST protegida por API Key, creando cuentas de comercio automáticamente y sincronizando el estado de inhabilitación/reactivación.

### Funcionalidades principales

- Landing page pública con información del servicio, planes y formulario de registro
- Panel de administración protegido por autenticación
- Gestión de comercios: alta, edición, inhabilitación, reactivación y eliminación
- Gestión de suscripciones: creación, edición, cancelación y renovación (30 días)
- Registro de pagos con extensión automática del vencimiento
- Panel de control con KPIs (MRR, tasa de cobranza, ingresos, nuevos comercios)
- API REST documentada con Swagger/OpenAPI
- Integración con FoodSave API para crear cuentas y gestionar el estado de comercios

---

## 2. Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| Lenguaje | C# |
| ORM | Entity Framework Core (Code-First) |
| BD producción | PostgreSQL (Railway) |
| BD desarrollo | SQLite |
| Frontend | Razor (.cshtml) + Bootstrap 5 + CSS personalizado |
| Autenticación | ASP.NET Core Cookie Authentication |
| API REST | ASP.NET Core API Controllers + Swagger/OpenAPI |
| Integración externa | HttpClient + FoodSave API (Node.js) |
| Despliegue | Azure App Service + GitHub Actions |

---

## 3. Arquitectura del proyecto

```
Foodsave.Web/
├── Models/                     Entidades, ViewModels, DTOs, InputModels, Enums
│   ├── Comercio.cs             Entidad principal
│   ├── Titular.cs              Dueño (1:1 con Comercio)
│   ├── Suscripcion.cs          Plan contratado
│   ├── Pago.cs                 Registro contable
│   ├── SolicitudComercio.cs    Formulario público
│   ├── Enums.cs                5 enums tipados
│   ├── ApiDtos.cs              DTOs + mapeo ToDto()
│   └── ApiModels.cs            ApiError + inputs API
│
├── Data/
│   ├── ApplicationDbContext.cs          DbContext (5 DbSets)
│   ├── DatabaseConnectionStringResolver.cs
│   ├── DbInitializer.cs                 Migraciones + seed + normalización
│   └── Configurations/                  IEntityTypeConfiguration<T> por entidad
│       ├── ComercioConfiguration.cs
│       ├── TitularConfiguration.cs
│       ├── SuscripcionConfiguration.cs
│       ├── PagoConfiguration.cs
│       └── SolicitudComercioConfiguration.cs
│
├── Services/                   Capa de lógica de negocio
│   ├── AuthService.cs
│   ├── GestionSuscripcionesService.cs
│   ├── RegistroPagoService.cs
│   ├── EstadisticasService.cs
│   └── FoodSaveApiClient.cs
│
├── Controllers/
│   ├── HomeController.cs       Landing + error + 404
│   ├── AuthController.cs       Login/logout (MVC)
│   ├── ComerciosController.cs  CRUD comercios (MVC, partial)
│   ├── EstadisticasController.cs
│   ├── SolicitudesComercioController.cs
│   └── Api/                    Controladores REST
│       ├── ApiAuthController.cs
│       ├── ApiComerciosController.cs
│       ├── ApiEstadisticasController.cs
│       ├── PagosController.cs
│       └── SolicitudesController.cs
│
├── Views/                      Plantillas Razor (.cshtml)
├── Infrastructure/
│   ├── ApiExceptionFilter.cs
│   ├── InvariantDecimalModelBinder.cs
│   └── SecurityHeadersMiddleware.cs
├── Helpers/
│   ├── TextHelper.cs
│   ├── PlanHelper.cs
│   └── WhatsAppLinkHelper.cs
├── Migrations/                 8 migraciones EF Core
├── wwwroot/                    CSS, JS, imágenes
├── Program.cs                  Entry point
├── ServiceCollectionExtensions.cs
└── appsettings.json
```

### Patrones de diseño

| Patrón | Aplicación |
|--------|-----------|
| MVC | Separación Modelo-Vista-Controlador |
| Service Layer | Lógica de negocio en servicios inyectables |
| Repository implícito | DbContext + LINQ queries |
| DTO | Objetos planos para API, sin exponer entidades |
| Extension Methods | Servicios agrupados en ServiceCollectionExtensions |
| Partial Class | ComerciosController dividido en 2 archivos |
| IEntityTypeConfiguration<T> | Configuración EF Core por entidad (1 archivo por tabla) |

---

## 4. Modelo de datos

### 4.1 Entidad-Relación

```
Titular (1) ---- (1) Comercio (1) ---- (*) Suscripcion (1) ---- (*) Pago

SolicitudComercio (tabla independiente, formulario público)
```

### 4.2 Enums

| Enum | Valores | Columna BD |
|------|---------|-----------|
| EstadoAdministrativo | Activo, Inhabilitado, PendientePago | varchar(20) |
| EstadoSuscripcion | Activa, Pendiente, Vencida, Cancelada | varchar(20) |
| EstadoPagoSuscripcion | AlDia, Pendiente, Vencido | varchar(20) |
| PlanSuscripcion | Estandar, Pro | varchar(20) |
| EstadoSolicitud | Pendiente, Aceptada, Rechazada | varchar(20) |

Todos los enums usan HasConversion<string>() para almacenarse como texto legible en la BD.

### 4.3 Índices

| Tabla | Columnas | Tipo |
|-------|---------|------|
| Titulares | Email | UNIQUE |
| Comercios | Nombre | UNIQUE |
| Comercios | EstadoAdministrativo | Normal |
| Comercios | Rubro | Normal |
| Suscripciones | Estado | Normal |
| Suscripciones | EstadoPago | Normal |
| Suscripciones | FechaProximoVencimiento | Normal |
| Pagos | (ComercioId, SuscripcionId, FechaPago) | Compuesto |
| SolicitudesComercio | Estado | Normal |
| SolicitudesComercio | FechaSolicitud | Normal |
| SolicitudesComercio | EmailTitular | Normal |

### 4.4 Restricciones de integridad

- decimal(12,2) para montos monetarios
- timestamp without time zone para fechas de negocio
- timestamp with time zone para fechas de auditoría
- NOT NULL en todas las columnas críticas
- MaxLength en todos los strings
- FK con DeleteBehavior.Restrict (protección de datos)

### 4.5 Migraciones (8 archivos, orden cronológico)

| # | Nombre | Descripción |
|---|--------|-------------|
| 1 | InitialPostgreSql | Tablas base (Titulares, Comercios, Suscripciones) |
| 2 | AddSolicitudesComercio | Tabla de solicitudes públicas |
| 3 | AddGestionAdministrativaPagosEstadisticas | Estados admin, pagos, backfill SQL |
| 4 | MakeComercioIdRequired | FK ComercioId NOT NULL + Restrict |
| 5 | MejorasModeloDatos | Índices únicos, enums, MaxLength |
| 6 | FechaFinNullable | FechaFin opcional (servicio mes a mes) |
| 7 | AddCiudadYContrasena | Campos para integración API |
| 8 | AddFoodSaveBusinessId | Campo FoodSaveBusinessId en Comercios |

---

## 5. API REST

### 5.1 Endpoints MVC (interfaz web)

| Método | Ruta | Auth |
|--------|------|------|
| GET | / | No |
| GET/POST | /Auth/Login | No |
| POST | /Auth/Logout | Sí |
| GET | /Comercios | Sí |
| GET | /Comercios/Details/{id} | Sí |
| GET/POST | /Comercios/Create | Sí |
| GET/POST | /Comercios/Editar/{id} | Sí |
| POST | /Comercios/Inhabilitar/{id} | Sí |
| POST | /Comercios/Reactivar/{id} | Sí |
| POST | /Comercios/Eliminar/{id} | Sí |
| GET | /Comercios/Suscripcion/{id} | Sí |
| POST | /Comercios/EditarSuscripcion/{id} | Sí |
| POST | /Comercios/CancelarSuscripcion/{id} | Sí |
| POST | /Comercios/CrearSuscripcion/{id} | Sí |
| GET/POST | /Comercios/RegistrarPago/{id} | Sí |
| GET | /SolicitudesComercio | Sí |
| GET | /SolicitudesComercio/Details/{id} | Sí |
| GET/POST | /SolicitudesComercio/Create | No |
| POST | /SolicitudesComercio/Aceptar/{id} | Sí |
| POST | /SolicitudesComercio/Rechazar/{id} | Sí |
| GET | /Estadisticas | Sí |
| GET | /health | No |

### 5.2 Endpoints API REST

| Método | Ruta | Auth |
|--------|------|------|
| POST | /api/auth/login | No |
| POST | /api/auth/logout | Sí |
| GET | /api/comercios | Sí |
| GET | /api/comercios/{id} | Sí |
| POST | /api/comercios | Sí |
| PATCH | /api/comercios/{id}/estado | Sí |
| GET | /api/comercios/{id}/pagos | Sí |
| GET | /api/solicitudes | Sí |
| GET | /api/solicitudes/{id} | Sí |
| POST | /api/solicitudes | No |
| PATCH | /api/solicitudes/{id} | Sí |
| POST | /api/pagos | Sí |
| GET | /api/estadisticas | Sí |

### 5.3 Documentación Swagger

Disponible en desarrollo: https://localhost:7206/swagger

### 5.4 Formato de errores API

```json
// 404
{ "error": "Comercio no encontrado." }

// 400 - Validación
{ "error": "Error de validación.", "detalles": { "campo": ["mensaje"] } }

// 400 - Negocio
{ "error": "La solicitud ya fue revisada." }

// 500 - Error interno (ApiExceptionFilter)
{ "status": 500, "title": "Error interno del servidor.", "detail": "..." }
```

---

## 6. Seguridad

### 6.1 Headers de seguridad (SecurityHeadersMiddleware)

| Header | Valor |
|--------|-------|
| Content-Security-Policy | default-src 'self'; img-src 'self' https://res.cloudinary.com; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; frame-ancestors 'none' |
| X-Content-Type-Options | nosniff |
| X-Frame-Options | DENY |
| Referrer-Policy | strict-origin-when-cross-origin |
| Permissions-Policy | camera=(), microphone=(), geolocation=() |

### 6.2 Otras medidas

| Medida | Implementación |
|--------|---------------|
| CSRF | AutoValidateAntiforgeryToken global en MVC. [IgnoreAntiforgeryToken] en controladores API |
| Rate Limiting | Login: 5 intentos / 15 min. Formulario público: 5 / 10 min |
| HSTS | Habilitado en producción |
| HTTPS | Redirección automática |
| Cookie security | HttpOnly=true, SameSite=Lax, SecurePolicy=SameAsRequest, 8h sliding |
| SQL Injection | EF Core parametrized queries |
| XSS | Auto-encode + CSP |
| Input validation | Data Annotations en todos los InputModels |
| API Key | X-API-Key header vía HttpClient config |

---

## 7. Integración con FoodSave API

### Crear cuenta de comercio

```
POST https://foodsave-unti.onrender.com/auth/register-business
Headers: X-API-Key: {apiKey}
Body: { email, password, businessName, businessAddress, businessCategory, businessCity, ownerName, ownerPhone }
Response 201: { "success": true, "data": { "user": { "businessId": "..." } } }
```

El businessId se almacena en Comercios.FoodSaveBusinessId.

### Suspender / reactivar comercio

```
PATCH https://foodsave-unti.onrender.com/auth/register-business/toggle-active
Headers: X-API-Key: {apiKey}
Body: { "businessId": "...", "isActive": false }
```

Se llama automáticamente al inhabilitar/reactivar.

---

## 8. Despliegue

### Producción (Azure + Railway PostgreSQL)

Variables de entorno requeridas en Azure Portal:

```
DATABASE_URL=postgresql://usuario:password@host:5432/foodsave?sslmode=require
ASPNETCORE_ENVIRONMENT=Production
DemoAuth__Email=admin@foodsave.com
DemoAuth__Password=passwordSeguro
FoodSaveApi__BaseUrl=https://foodsave-unti.onrender.com
FoodSaveApi__ApiKey=FsXk9mP2vL7nQ4wR8yT1bD6cJ3aH5gE0
```

1. GitHub Actions: Build Release + deploy a Azure Web App Foodsave
2. Base de datos: PostgreSQL en Railway (DATABASE_URL)
3. Migraciones: se aplican automáticamente al iniciar

### Desarrollo local

```powershell
cd Foodsave.Web
dotnet run --launch-profile https
```

- URL: https://localhost:7206
- BD: SQLite (foodsave_dev.db)
- Swagger: https://localhost:7206/swagger
- Health: https://localhost:7206/health

---

## 9. Panel de control (Estadísticas)

| Métrica | Descripción |
|---------|-------------|
| MRR | Suma de montos mensuales de suscripciones activas |
| Cobrado este mes | Pagos registrados en el mes actual |
| Tasa de cobranza | % de comercios que pagaron este mes |
| Comercios activos | Total con estado Activo |
| Al día / Pendientes / Inhabilitados | Distribución por estado |
| Ingresos anuales | Total de pagos en el año actual |
| Nuevos comercios | Altas en el mes y año actual |
| Solicitudes pendientes | Sin revisar + recibidas este mes |

---

## 10. Credenciales de acceso

Las credenciales no están visibles en el código ni en la interfaz.
Se configuran mediante:

| Entorno | Ubicación |
|---------|-----------|
| Desarrollo local | launchSettings.json (variables de entorno) |
| Producción Azure | Azure Portal > App Service > Environment Variables |
| .env local | Archivo .env (no versionado, en .gitignore) |

---

## 11. Comandos útiles

```powershell
# Restaurar herramientas EF
dotnet tool restore

# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Ejecutar en desarrollo
dotnet run --launch-profile https

# Build release
dotnet build --configuration Release
```
