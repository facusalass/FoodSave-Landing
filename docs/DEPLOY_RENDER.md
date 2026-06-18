# Deploy de FoodSave en Render

FoodSave se despliega como un **Web Service** Docker conectado a una base
**Render Postgres**. La aplicación usa PostgreSQL con Npgsql, aplica las
migraciones pendientes al iniciar y carga el seed de forma idempotente.

## 1. Crear PostgreSQL

1. En el Dashboard de Render, seleccionar **New > Postgres**.
2. Elegir un nombre, región y plan.
3. Crear la base y esperar a que quede disponible.
4. En **Connect**, copiar la **Internal Database URL**.

La base y el Web Service deben estar en la misma cuenta y región para usar la
red privada. La URL interna reduce latencia y evita sacar el tráfico de base de
datos a internet. La URL externa se reserva para herramientas ejecutadas fuera
de Render.

Documentación oficial:

- <https://render.com/docs/postgresql-creating-connecting>

## 2. Crear el Web Service

1. Seleccionar **New > Web Service**.
2. Conectar la cuenta de GitHub, GitLab o Bitbucket.
3. Elegir este repositorio y la rama que se desplegará.
4. Seleccionar **Docker** como runtime.
5. Confirmar que Render use el `Dockerfile` ubicado en la raíz.
6. Elegir la misma región configurada para PostgreSQL.
7. Configurar `/` como **Health Check Path**.

El Dockerfile realiza restore y publish con .NET 10 y ejecuta
`Foodsave.Web.dll`. No requiere Build Command ni Start Command adicionales.

Render inyecta `PORT`. `Program.cs` hace que la aplicación escuche por HTTP en
`0.0.0.0:$PORT`, como requiere un Web Service. No se debe fijar un puerto en el
Dockerfile ni en las variables del servicio.

Documentación oficial:

- <https://render.com/docs/web-services>
- <https://render.com/docs/docker>

## 3. Configurar variables de entorno

En el Web Service, abrir **Environment** y agregar:

```text
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=<Internal Database URL de Render Postgres>
```

`DATABASE_URL` admite URLs con esquema `postgres://` o `postgresql://`. Se debe
copiar la URL interna entregada por Render sin publicar su valor en Git.

Como alternativa, se puede omitir `DATABASE_URL` y configurar:

```text
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

La aplicación resuelve la conexión en este orden:

1. `ConnectionStrings__DefaultConnection`
2. `DATABASE_URL`

No conviene definir ambas salvo que se quiera que la connection string Npgsql
tenga precedencia. Los ejemplos sin credenciales reales están en
`.env.example`; ASP.NET Core no carga ese archivo automáticamente.

Render administra `PORT`, por lo que no debe agregarse manualmente.

Documentación oficial:

- <https://render.com/docs/configure-environment-variables>

## 4. Desplegar

Guardar las variables y crear el Web Service. Render construirá la imagen desde
el Dockerfile y mostrará el progreso en **Events** y **Logs**.

Al iniciar, `DbInitializer` ejecuta:

1. `Database.Migrate()`.
2. El seed idempotente de comercios de demostración.

Las migraciones PostgreSQL versionadas son:

- `InitialPostgreSql`
- `AddSolicitudesComercio`

No hay fallback a SQLite.

Con una única instancia, las migraciones pueden ejecutarse durante el arranque.
Antes de escalar a varias réplicas, conviene moverlas a un proceso de
pre-deploy o release para evitar que más de una instancia intente aplicarlas al
mismo tiempo.

## 5. Revisar logs

En **Logs**, confirmar:

- que la imagen se construya correctamente;
- que la aplicación escuche en el puerto asignado por Render;
- que no haya errores de conexión a PostgreSQL;
- que las migraciones terminen correctamente;
- que el servicio quede disponible en su dominio `onrender.com`.

Si falla la conexión, revisar que el Web Service y PostgreSQL estén en la misma
región y que `DATABASE_URL` contenga la URL interna completa.

## 6. Verificación funcional

Después del deploy:

1. Abrir `/` y comprobar que la landing responda.
2. Abrir `/SolicitudesComercio/Create` y comprobar el formulario público.
3. Abrir `/Comercios` sin sesión y confirmar la redirección a `/Auth/Login`.
4. Ingresar con `comercio@foodsave.com` / `123456`.
5. Confirmar que `/Comercios` muestre los datos almacenados en PostgreSQL.
6. Probar el alta manual de un comercio.
7. Cerrar sesión.
8. Volver a `/Comercios` y confirmar que requiera autenticación.

## Render Blueprint

No se incluye `render.yaml` por ahora. El servicio web y PostgreSQL pueden
configurarse de forma simple desde el Dashboard, sin fijar en el repositorio
nombres, región ni plan. Si más adelante se necesita infraestructura
reproducible para varios ambientes, se puede incorporar un Blueprint.
