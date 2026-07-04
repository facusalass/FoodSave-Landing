namespace Foodsave.Web.Infrastructure
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // CSP: solo recursos de nuestro dominio + imágenes de Cloudinary
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "img-src 'self' https://res.cloudinary.com; " +
                "style-src 'self' 'unsafe-inline'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "frame-ancestors 'none';";

            // Evita que el navegador intente adivinar el content-type
            headers["X-Content-Type-Options"] = "nosniff";

            // Previene clickjacking
            headers["X-Frame-Options"] = "DENY";

            // No enviar el referer completo a sitios externos
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Deshabilitada (obsoleta, reemplazada por CSP)
            headers["X-XSS-Protection"] = "0";

            // Restringe APIs del navegador (cámara, mic, etc.)
            headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            await _next(context);
        }
    }
}
