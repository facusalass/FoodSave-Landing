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
- Procesamiento de pagos reales, checkout o integraciones con proveedores.
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

## Gestión administrativa de comercios, pagos y estadísticas

El panel autenticado permite controlar el estado administrativo de cada
comercio como `Activo`, `Inhabilitado` o `PendientePago`. Una inhabilitación es
manual y no se revierte al registrar un cobro; al reactivar, el estado final se
determina según la situación de la suscripción.

La entidad `Suscripcion` conserva el plan y agrega monto mensual configurable,
último pago, próximo vencimiento y estado de pago (`AlDia`, `Pendiente` o
`Vencido`). No existen precios oficiales precargados: los importes pueden
comenzar en cero y el administrador los configura manualmente.

Los cobros internos se registran en `Pago` desde el detalle de un comercio.
Este registro no procesa dinero, no ofrece checkout y no se integra con Mercado
Pago ni con otro proveedor. Registrar un pago actualiza la suscripción, su
vencimiento y el estado de pago. Marcar una suscripción al día sin cobro no
genera ingresos registrados.

La ruta protegida `/Estadisticas` resume comercios por estado, suscripciones al
día o vencidas, solicitudes pendientes, cantidad de pagos, ingresos mensuales
estimados e ingresos registrados totales. Los ingresos estimados usan los
montos configurados de suscripciones vigentes y los ingresos registrados suman
exclusivamente filas de `Pago`.
