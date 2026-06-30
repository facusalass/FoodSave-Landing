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
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 1, 10), FechaFin = new DateTime(2026, 7, 10) },
                        new Suscripcion { Plan = PlanSuscripcion.Pro, Estado = EstadoSuscripcion.Pendiente, FechaInicio = new DateTime(2026, 7, 11), FechaFin = new DateTime(2027, 1, 11) }
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
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 2, 1), FechaFin = new DateTime(2026, 8, 1) },
                        new Suscripcion { Plan = PlanSuscripcion.Pro, Estado = EstadoSuscripcion.Pendiente, FechaInicio = new DateTime(2026, 8, 2), FechaFin = new DateTime(2027, 2, 2) }
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
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 3, 5), FechaFin = new DateTime(2026, 9, 5) },
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Vencida, FechaInicio = new DateTime(2025, 8, 1), FechaFin = new DateTime(2026, 2, 1) }
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
                        new Suscripcion { Plan = PlanSuscripcion.Pro, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 1, 20), FechaFin = new DateTime(2026, 7, 20) },
                        new Suscripcion { Plan = PlanSuscripcion.Pro, Estado = EstadoSuscripcion.Pendiente, FechaInicio = new DateTime(2026, 7, 21), FechaFin = new DateTime(2027, 1, 21) }
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
                        new Suscripcion { Plan = PlanSuscripcion.Estandar, Estado = EstadoSuscripcion.Vencida, FechaInicio = new DateTime(2025, 10, 15), FechaFin = new DateTime(2026, 4, 15) },
                        new Suscripcion { Plan = PlanSuscripcion.Pro, Estado = EstadoSuscripcion.Activa, FechaInicio = new DateTime(2026, 4, 20), FechaFin = new DateTime(2026, 10, 20) }
                    }
                }
            };

            var montosPorPlan = new Dictionary<PlanSuscripcion, decimal>
            {
                [PlanSuscripcion.Estandar] = 0m,
                [PlanSuscripcion.Pro] = 1500m
            };

            foreach (var comercio in comercios)
            {
                comercio.EstadoAdministrativo = EstadoAdministrativo.Activo;

                foreach (var suscripcion in comercio.Suscripciones)
                {
                    suscripcion.MontoMensual = montosPorPlan[suscripcion.Plan];
                    suscripcion.FechaProximoVencimiento = suscripcion.FechaFin;
                    suscripcion.FechaUltimoPago =
                        suscripcion.Estado == EstadoSuscripcion.Activa
                            ? suscripcion.FechaInicio
                            : null;
                    suscripcion.EstadoPago = suscripcion.Estado switch
                    {
                        EstadoSuscripcion.Activa => EstadoPagoSuscripcion.AlDia,
                        EstadoSuscripcion.Vencida => EstadoPagoSuscripcion.Vencido,
                        _ => EstadoPagoSuscripcion.Pendiente
                    };
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
        }

        private static void AgregarPagosSemilla(ApplicationDbContext context)
        {
            if (context.Pagos.Any())
                return;

            var suscripciones = context.Suscripciones
                .Where(s => s.Estado == EstadoSuscripcion.Activa)
                .ToList();

            foreach (var suscripcion in suscripciones)
            {
                var fechaPago = suscripcion.FechaInicio.AddMonths(1);
                context.Pagos.Add(new Pago
                {
                    ComercioId = suscripcion.ComercioId,
                    SuscripcionId = suscripcion.Id,
                    Monto = suscripcion.MontoMensual,
                    FechaPago = fechaPago,
                    Observacion = "Pago registrado automáticamente (demostración)"
                });
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
