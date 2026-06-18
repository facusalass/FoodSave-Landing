using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

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
                        new Suscripcion { Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 1, 10), FechaFin = new DateTime(2026, 7, 10) },
                        new Suscripcion { Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 7, 11), FechaFin = new DateTime(2027, 1, 11) }
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
                        new Suscripcion { Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 2, 1), FechaFin = new DateTime(2026, 8, 1) },
                        new Suscripcion { Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 8, 2), FechaFin = new DateTime(2027, 2, 2) }
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
                        new Suscripcion { Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 3, 5), FechaFin = new DateTime(2026, 9, 5) },
                        new Suscripcion { Plan = "Estandar", Estado = "Vencida", FechaInicio = new DateTime(2025, 8, 1), FechaFin = new DateTime(2026, 2, 1) }
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
                        new Suscripcion { Plan = "Pro", Estado = "Activa", FechaInicio = new DateTime(2026, 1, 20), FechaFin = new DateTime(2026, 7, 20) },
                        new Suscripcion { Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 7, 21), FechaFin = new DateTime(2027, 1, 21) }
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
                        new Suscripcion { Plan = "Estandar", Estado = "Vencida", FechaInicio = new DateTime(2025, 10, 15), FechaFin = new DateTime(2026, 4, 15) },
                        new Suscripcion { Plan = "Pro", Estado = "Activa", FechaInicio = new DateTime(2026, 4, 20), FechaFin = new DateTime(2026, 10, 20) }
                    }
                }
            };

            foreach (var comercio in comercios)
            {
                comercio.EstadoAdministrativo = Comercio.EstadoActivo;

                foreach (var suscripcion in comercio.Suscripciones)
                {
                    suscripcion.MontoMensual = 0;
                    suscripcion.FechaProximoVencimiento = suscripcion.FechaFin;
                    suscripcion.FechaUltimoPago =
                        suscripcion.Estado == Suscripcion.EstadoActiva
                            ? suscripcion.FechaInicio
                            : null;
                    suscripcion.EstadoPago = suscripcion.Estado switch
                    {
                        Suscripcion.EstadoActiva =>
                            Suscripcion.EstadoPagoAlDia,
                        Suscripcion.EstadoVencida =>
                            Suscripcion.EstadoPagoVencido,
                        _ => Suscripcion.EstadoPagoPendiente
                    };
                }
            }

            var nombresExistentes = context.Comercios
                .Select(comercio => comercio.Nombre)
                .ToHashSet();
            var comerciosFaltantes = comercios
                .Where(comercio => !nombresExistentes.Contains(comercio.Nombre))
                .ToList();

            if (comerciosFaltantes.Count == 0)
            {
                return;
            }

            context.Comercios.AddRange(comerciosFaltantes);
            context.SaveChanges();
        }
    }
}
