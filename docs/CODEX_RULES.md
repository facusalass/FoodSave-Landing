# Reglas técnicas del proyecto

- Trabajar únicamente sobre la landing ASP.NET Core MVC de `Foodsave.Web`.
- No mezclar esta solución con aplicaciones mobile o Expo.
- Mantener PostgreSQL como único proveedor de Entity Framework Core.
- No hardcodear hosts, usuarios, contraseñas ni puertos de base de datos.
- No subir archivos `.env`, bases locales ni secretos.
- Mantener el login con cookies y la protección de `/Comercios`.
- No modificar la UI salvo pedido explícito.
- Mantener el seed idempotente y las migraciones compatibles con PostgreSQL.
- Ejecutar `dotnet build` y pruebas de autenticación/comercios antes de cerrar.
- Actualizar la documentación cuando cambien despliegue, variables o proveedor.
