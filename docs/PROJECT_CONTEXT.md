# FoodSave Landing - Contexto del proyecto

FoodSave Landing es una aplicación web ASP.NET Core MVC para presentar la
propuesta FoodSave y administrar comercios gastronómicos.

## Arquitectura actual

- Proyecto principal: `Foodsave.Web`.
- Framework: ASP.NET Core MVC sobre .NET 10.
- Persistencia: PostgreSQL con Entity Framework Core y Npgsql.
- Entidades persistidas: `Comercio`, `Titular`, `Suscripcion` y
  `SolicitudComercio`.
- Migraciones: EF Core, con migración inicial PostgreSQL y
  `AddSolicitudesComercio`.
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

El proveedor objetivo de despliegue es Render. Para desarrollo y despliegue,
consultar `docs/DEPLOY_RENDER.md`. La guía de Railway se conserva únicamente
como referencia histórica y ya no describe el destino actual.

## Solicitudes públicas de comercios

Los comercios interesados pueden enviar una solicitud sin tener usuario ni
acceso al panel:

- Formulario público: `/SolicitudesComercio/Create`.
- Confirmación pública: `/SolicitudesComercio/Confirmacion`.
- Listado administrativo protegido: `/SolicitudesComercio`.
- Detalle administrativo protegido:
  `/SolicitudesComercio/Details/{id}`.

Cada solicitud se guarda inicialmente como `Pendiente`. El administrador puede
marcarla como `Aceptada` o `Rechazada`, agregar una observación interna y
contactar al responsable por WhatsApp cuando el teléfono tiene un formato
válido.

Aceptar una solicitud no crea automáticamente el comercio. El detalle muestra
un acceso al formulario administrativo existente para completar el alta
manualmente y preservar ese flujo. Rechazar una solicitud conserva el registro
y su fecha de revisión.
