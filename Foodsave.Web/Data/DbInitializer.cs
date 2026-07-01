using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Database.IsNpgsql())
            {
                context.Database.Migrate();
            }
            else
            {
                context.Database.EnsureCreated();
            }

            var comercios = new List<Comercio>
            {
                new Comercio
                {
                    Nombre = "Panader\u00EDa La Espiga",
                    Rubro = "Panader\u00EDa",
                    Direccion = "Av. San Martin 1200",
                    Telefono = "11-4567-1001",
                    Titular = new Titular
                    {
                        Nombre = "Laura",
                        Apellido = "Gomez",
                        Email = "laura.gomez@email.com",
                        Telefono = "11-3000-1001"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 1, 10) }
                    }
                },
                new Comercio
                {
                    Nombre = "Rotiser\u00EDa Don Carlos",
                    Rubro = "Rotiser\u00EDa",
                    Direccion = "Calle Moreno 850",
                    Telefono = "11-4567-1002",
                    Titular = new Titular
                    {
                        Nombre = "Carlos",
                        Apellido = "Perez",
                        Email = "carlos.perez@email.com",
                        Telefono = "11-3000-1002"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 2, 1) }
                    }
                },
                new Comercio
                {
                    Nombre = "Verduleria El Fresco",
                    Rubro = "Verduleria",
                    Direccion = "Las Heras 510",
                    Telefono = "11-4567-1003",
                    Titular = new Titular
                    {
                        Nombre = "Sofia",
                        Apellido = "Diaz",
                        Email = "sofia.diaz@email.com",
                        Telefono = "11-3000-1003"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 3, 5) }
                    }
                },
                new Comercio
                {
                    Nombre = "Caf\u00E9 Centro",
                    Rubro = "Cafeter\u00EDa",
                    Direccion = "Mitre 980",
                    Telefono = "11-4567-1004",
                    Titular = new Titular
                    {
                        Nombre = "Nicolas",
                        Apellido = "Ruiz",
                        Email = "nicolas.ruiz@email.com",
                        Telefono = "11-3000-1004"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 1, 20) }
                    }
                },
                new Comercio
                {
                    Nombre = "Restaurante Sabores",
                    Rubro = "Restaurante",
                    Direccion = "Belgrano 1425",
                    Telefono = "11-4567-1005",
                    Titular = new Titular
                    {
                        Nombre = "Carolina",
                        Apellido = "Lopez",
                        Email = "carolina.lopez@email.com",
                        Telefono = "11-3000-1005"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 4, 20) }
                    }
                }
            };

            var montosPorPlan = new Dictionary<PlanSuscripcion, decimal>
            {
                [PlanSuscripcion.Estandar] = 20000m
            };

            foreach (var comercio in comercios)
            {
                comercio.EstadoAdministrativo = EstadoAdministrativo.Activo;

                foreach (var suscripcion in comercio.Suscripciones)
                {
                    if (suscripcion.Estado == EstadoSuscripcion.Activa)
                    {
                        var inicio = DateTime.Today.AddDays(-45);
                        suscripcion.FechaInicio = inicio;
                        suscripcion.FechaFin = null;
                        suscripcion.FechaUltimoPago = DateTime.Today.AddDays(-15);
                        suscripcion.FechaProximoVencimiento = DateTime.Today.AddDays(15);
                        suscripcion.EstadoPago = EstadoPagoSuscripcion.AlDia;
                    }
                    else
                    {
                        suscripcion.FechaFin = suscripcion.FechaInicio.AddDays(30);
                        suscripcion.MontoMensual = montosPorPlan[suscripcion.Plan];
                        suscripcion.FechaProximoVencimiento = suscripcion.FechaInicio.AddDays(30);
                        suscripcion.FechaUltimoPago = null;
                        suscripcion.EstadoPago = suscripcion.Estado switch
                        {
                            EstadoSuscripcion.Vencida => EstadoPagoSuscripcion.Vencido,
                            _ => EstadoPagoSuscripcion.Pendiente
                        };
                    }

                    suscripcion.MontoMensual = montosPorPlan[suscripcion.Plan];
                }
            }

            var nombresExistentes = context.Comercios
                .Select(comercio => comercio.Nombre)
                .ToHashSet();
            var comerciosFaltantes = comercios
                .Where(comercio => !nombresExistentes.Contains(comercio.Nombre))
                .ToList();

            if (comerciosFaltantes.Count > 0)
            {
                context.Comercios.AddRange(comerciosFaltantes);
                context.SaveChanges();

                AgregarPagosSemilla(context);
                AgregarSolicitudesSemilla(context);
            }

            NormalizarDatosExistentes(context);
        }

        private static void NormalizarDatosExistentes(ApplicationDbContext context)
        {
            var hoy = DateTime.Today;

            var necesitaNormalizacion = context.Suscripciones.Any(s =>
                s.MontoMensual == 0 ||
                s.Estado == EstadoSuscripcion.Pendiente ||
                (s.Estado == EstadoSuscripcion.Cancelada &&
                    (s.FechaInicio > hoy ||
                     (s.FechaFin != null && s.FechaFin.Value > hoy))) ||
                (s.Estado == EstadoSuscripcion.Activa &&
                    (s.FechaFin != null ||
                     (s.FechaProximoVencimiento - hoy).Days > 60)));

            if (!necesitaNormalizacion)
                return;

            var suscripciones = context.Suscripciones
                .Include(s => s.Comercio)
                .ToList();

            var cambios = false;

            // Agrupar por comercio para consistencia entre cancelada y activa
            foreach (var grupo in suscripciones.GroupBy(s => s.ComercioId))
            {
                var activa = grupo.FirstOrDefault(s => s.Estado == EstadoSuscripcion.Activa);
                var fechaAnterior = activa?.FechaInicio.AddDays(-1) ?? hoy;

                foreach (var suscripcion in grupo)
                {
                    // MontoMensual 0 → 20000
                    if (suscripcion.MontoMensual == 0)
                    {
                        suscripcion.MontoMensual = 20000m;
                        cambios = true;
                    }

                    // Pendiente del modelo viejo → Cancelada
                    if (suscripcion.Estado == EstadoSuscripcion.Pendiente)
                    {
                        suscripcion.Estado = EstadoSuscripcion.Cancelada;
                        suscripcion.FechaFin = fechaAnterior;
                        if (suscripcion.FechaInicio > fechaAnterior)
                            suscripcion.FechaInicio = fechaAnterior;
                        cambios = true;
                    }

                    // Cancelada con FechaInicio futura → corregir
                    if (suscripcion.Estado == EstadoSuscripcion.Cancelada &&
                        suscripcion.FechaInicio > fechaAnterior)
                    {
                        suscripcion.FechaInicio = fechaAnterior;
                        cambios = true;
                    }

                    // Cancelada con FechaFin futura → corregir
                    if (suscripcion.Estado == EstadoSuscripcion.Cancelada &&
                        suscripcion.FechaFin is not null &&
                        suscripcion.FechaFin.Value > fechaAnterior)
                    {
                        suscripcion.FechaFin = fechaAnterior;
                        cambios = true;
                    }

                    // Activa con FechaFin no null → null
                    if (suscripcion.Estado == EstadoSuscripcion.Activa && suscripcion.FechaFin is not null)
                    {
                        suscripcion.FechaFin = null;
                        cambios = true;
                    }

                    // Activa con FechaProximoVencimiento > 60 días (modelo 6 meses)
                    if (suscripcion.Estado == EstadoSuscripcion.Activa &&
                        (suscripcion.FechaProximoVencimiento - hoy).Days > 60)
                    {
                        suscripcion.FechaProximoVencimiento = hoy.AddDays(30);
                        cambios = true;
                    }
                }
            }

            if (cambios)
                context.SaveChanges();
        }

        private static void AgregarPagosSemilla(ApplicationDbContext context)
        {
            if (context.Pagos.Any())
                return;

            var suscripciones = context.Suscripciones
                .Include(s => s.Comercio)
                .Where(s => s.Estado == EstadoSuscripcion.Activa)
                .OrderBy(s => s.ComercioId)
                .ToList();

            var hoy = DateTime.Today;

            foreach (var suscripcion in suscripciones)
            {
                DateTime? ultimoPago = null;
                var fechaCursor = suscripcion.FechaInicio.AddDays(30);
                var mesesTotales = 0;

                while (fechaCursor <= hoy &&
                       (suscripcion.FechaFin == null || fechaCursor <= suscripcion.FechaFin))
                {
                    mesesTotales++;
                    fechaCursor = fechaCursor.AddDays(30);
                }

                var mesesAPagar = Math.Min(mesesTotales, 5);
                fechaCursor = suscripcion.FechaInicio.AddDays(30);

                for (int i = 0; i < mesesAPagar; i++)
                {
                    context.Pagos.Add(new Pago
                    {
                        ComercioId = suscripcion.ComercioId,
                        SuscripcionId = suscripcion.Id,
                        Monto = suscripcion.MontoMensual,
                        FechaPago = fechaCursor,
                        Observacion = $"Pago mensual {fechaCursor:yyyy-MM}"
                    });

                    ultimoPago = fechaCursor;
                    fechaCursor = fechaCursor.AddDays(30);
                }

                if (ultimoPago.HasValue)
                {
                    suscripcion.FechaUltimoPago = ultimoPago;
                    suscripcion.FechaProximoVencimiento = ultimoPago.Value.AddDays(30);
                }
            }

            context.SaveChanges();
        }

        private static void AgregarSolicitudesSemilla(ApplicationDbContext context)
        {
            if (context.SolicitudesComercio.Any())
                return;

            var solicitudes = new List<SolicitudComercio>
            {
                new SolicitudComercio
                {
                    NombreComercio = "Helader\u00EDa Grido",
                    Rubro = "Helader\u00EDa",
                    Direccion = "Av. Belgrano 2200",
                    TelefonoComercio = "11-4567-2001",
                    NombreTitular = "Mart\u00EDn",
                    ApellidoTitular = "Fern\u00E1ndez",
                    EmailTitular = "martin.fernandez@email.com",
                    PlanInteres = PlanSuscripcion.Estandar,
                    Mensaje = "Queremos ofrecer helado artesanal con descuento al final del d\u00EDa.",
                    Estado = EstadoSolicitud.Pendiente,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-3)
                },
                new SolicitudComercio
                {
                    NombreComercio = "Pizzer\u00EDa La N\u00E1poli",
                    Rubro = "Pizzer\u00EDa",
                    Direccion = "Calle San Juan 450",
                    TelefonoComercio = "11-4567-2002",
                    NombreTitular = "Giovanni",
                    ApellidoTitular = "Rossi",
                    EmailTitular = "giovanni.rossi@email.com",
                    PlanInteres = PlanSuscripcion.Pro,
                    Estado = EstadoSolicitud.Aceptada,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-7),
                    FechaRevision = DateTime.UtcNow.AddDays(-5),
                    ObservacionAdmin = "Comercio con gran potencial. Se coordin\u00F3 alta telef\u00F3nica."
                },
                new SolicitudComercio
                {
                    NombreComercio = "Kiosco 24hs El R\u00E1pido",
                    Rubro = "Kiosco",
                    Direccion = "Rivadavia 780",
                    TelefonoComercio = "11-4567-2003",
                    NombreTitular = "Ana",
                    ApellidoTitular = "Garc\u00EDa",
                    EmailTitular = "ana.garcia@email.com",
                    Estado = EstadoSolicitud.Rechazada,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-10),
                    FechaRevision = DateTime.UtcNow.AddDays(-8),
                    ObservacionAdmin = "No cumple con el rubro gastron\u00F3mico requerido."
                }
            };

            context.SolicitudesComercio.AddRange(solicitudes);
            context.SaveChanges();
        }
    }
}
