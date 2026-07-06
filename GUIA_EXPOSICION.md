# Guía de Exposición — FoodSave Landing

## División

| Persona | Módulos | Puntos |
|---------|---------|--------|
| **Vos** | 1. Modelo de datos + 3. Interfaz de usuario | 45 pts |
| **Amigo** | 2. API REST + 4. Maestro-detalle | 40 pts |

> El módulo 5 (Seguridad) se reparte entre ambos al final.

---

## VOS — Módulo 1: Modelo de datos (25 pts) | 60 segundos

### Archivos a mostrar (SOLO 3)

| Orden | Archivo | Qué hacer |
|-------|---------|-----------|
| 1 | Abrir `DatabaseConnectionStringResolver.cs` | Señalar que resuelve `DATABASE_URL` (línea 17-22) — es la conexión a PostgreSQL en Railway, la base de datos real en la nube |
| 2 | Abrir `ApplicationDbContext.cs` | Señalar las 5 DbSets (línea 13-17) y el OnModelCreating de 3 líneas |
| 3 | Abrir `Migrations/` | Mostrar que hay 8 migraciones en orden |

### Qué decir (leé esto textual)

> "El proyecto usa **PostgreSQL en Railway** como base de datos en producción. La conexión se resuelve desde la variable `DATABASE_URL` mediante un resolver personalizado. Las 5 tablas están hechas con Entity Framework Code-First y configuradas con `IEntityTypeConfiguration`, una por archivo. El `OnModelCreating` es una sola línea que escanea todo el proyecto. En la base de datos usamos `decimal(12,2)` para montos, índices únicos en email y nombre, y enums que se guardan como texto legible. Los borrados están bloqueados con RESTRICT para no perder datos. Hay 8 migraciones, todas reversibles. La base en Railway se actualiza automáticamente con cada deploy porque las migraciones se aplican al iniciar la app."

---

## VOS — Módulo 3: Interfaz de usuario (20 pts) | 60 segundos

### Archivos a mostrar (SOLO 3)

| Orden | Archivo | Qué hacer |
|-------|---------|-----------|
| 1 | Abrir `Views/Shared/_Layout.cshtml` | Señalar el navbar con los links de navegación (línea 40-55 aprox) |
| 2 | Abrir `wwwroot/css/site.css` | Scroll rápido mostrando los colores naranja/verde y las clases |
| 3 | Abrir `Views/Estadisticas/Index.cshtml` | Mostrar el panel con tarjetas de MRR, tasa de cobranza, comercios activos |

### Qué decir (leé esto textual)

> "La interfaz usa Bootstrap 5 con una paleta de colores propia en naranja y verde. La navegación tiene breadcrumbs y resalta la página donde estás parado. Todas las páginas tienen indicadores de carga, tooltips en los botones, y mensajes de confirmación antes de acciones destructivas. El panel de control muestra métricas clave como ingresos mensuales, tasa de cobranza y nuevos comercios. Todo se comunica con el backend mediante servicios compartidos, tanto para las vistas HTML como para la API REST."

---

## AMIGO — Módulo 2: API REST (30 pts) | 60 segundos

### Archivos a mostrar (SOLO 3)

| Orden | Archivo | Qué hacer |
|-------|---------|-----------|
| 1 | Abrir carpeta `Controllers/Api/` | Mostrar los 5 controllers |
| 2 | Abrir `Controllers/Api/ComerciosController.cs` | Señalar `[HttpGet]`, `[HttpPost]`, `[Route("api/comercios")]` |
| 3 | Abrir `Services/EstadisticasService.cs` | Señalar las queries que calculan MRR, tasa de cobranza, etc. |

### Qué decir (leé esto textual)

> "Además de las vistas HTML, el proyecto expone una API REST completa con 6 controladores. Cada endpoint devuelve JSON usando DTOs para no exponer las entidades directamente. La validación se hace en 3 capas: Data Annotations en los modelos de entrada, validación automática de ASP.NET, y reglas de negocio en los servicios. Por ejemplo, el servicio de estadísticas calcula el ingreso mensual recurrente y la tasa de cobranza consultando directamente la base de datos. Los errores siempre tienen el mismo formato: 400 con detalles, 404 con mensaje, 500 con stack trace solo en desarrollo."

---

## AMIGO — Módulo 4: Maestro-detalle (10 pts) | 45 segundos

### Archivos a mostrar (SOLO 2)

| Orden | Archivo | Qué hacer |
|-------|---------|-----------|
| 1 | Abrir `Controllers/ComerciosController.cs` | Buscar el método `Create` y señalar que crea Comercio + Titular + Suscripción juntos (línea 139-155 aprox) |
| 2 | Abrir `Controllers/ComerciosController.Suscripciones.cs` | Señalar los métodos `CancelarSuscripcion` y `CrearSuscripcion` |

### Qué decir (leé esto textual)

> "Cuando se crea un comercio, automáticamente se crean sus datos relacionados: el titular y la primera suscripción. Para editar, un solo formulario modifica tanto el comercio como su titular. Las bajas son lógicas: inhabilitar no borra nada, solo cambia el estado. Si se necesita eliminar, hay que inhabilitar primero, y el sistema borra en orden todo dentro de una transacción. Las suscripciones tienen su propia página donde se puede cancelar la actual y crear una nueva, y en el historial solo aparecen las vencidas o canceladas."

---

## AMBOS — Módulo 5: Seguridad (10 pts) | 60 segundos

| Quién | Archivo | Qué decir |
|-------|---------|-----------|
| **Vos** | `Infrastructure/SecurityHeadersMiddleware.cs` | "Agregamos headers de seguridad: CSP para evitar XSS, X-Frame-Options contra clickjacking, y nosniff para que el navegador no adivine el tipo de contenido." |
| **Vos** | `ServiceCollectionExtensions.cs` línea 21 | "Todos los formularios tienen protección CSRF automática con AutoValidateAntiforgeryToken." |
| **Amigo** | `ServiceCollectionExtensions.cs` líneas 76-78 | "La cookie de sesión es HttpOnly, SameSite Lax, y expira a las 8 horas con extensión por actividad." |
| **Amigo** | `Controllers/AuthController.cs` línea 36 | "El login tiene rate limiting: máximo 5 intentos cada 15 minutos para evitar fuerza bruta." |

---

## Resumen de tiempos

| Minuto | Quién | Módulo |
|--------|-------|--------|
| 0:00 | Vos | Modelo de datos |
| 1:00 | Vos | Interfaz de usuario |
| 2:00 | Amigo | API REST |
| 3:00 | Amigo | Maestro-detalle |
| 3:45 | Ambos | Seguridad |
| **4:45** | | **Fin** |
