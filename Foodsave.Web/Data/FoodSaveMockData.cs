using Foodsave.Web.Models;

namespace Foodsave.Web.Data
{
    public static class FoodSaveMockData
    {
        public static List<Comercio> ObtenerComercios()
        {
            return new List<Comercio>
            {
                new Comercio
                {
                    Id = 1,
                    Nombre = "Panaderia El Trigal",
                    Rubro = "Panaderia",
                    Direccion = "Av. San Martin 1240",
                    Telefono = "11-4567-1234",
                    Titular = new Titular
                    {
                        Id = 1,
                        Nombre = "Laura",
                        Apellido = "Gomez",
                        Email = "laura.gomez@email.com",
                        Telefono = "11-3000-1122"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Id = 1, Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 1, 10), FechaFin = new DateTime(2026, 6, 10) },
                        new Suscripcion { Id = 2, Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 6, 11), FechaFin = new DateTime(2026, 12, 11) }
                    }
                },
                new Comercio
                {
                    Id = 2,
                    Nombre = "Rotiseria Don Tito",
                    Rubro = "Rotiseria",
                    Direccion = "Calle Moreno 835",
                    Telefono = "11-4567-2233",
                    Titular = new Titular
                    {
                        Id = 2,
                        Nombre = "Martin",
                        Apellido = "Perez",
                        Email = "martin.perez@email.com",
                        Telefono = "11-3000-2233"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Id = 3, Plan = "Estandar", Estado = "Vencida", FechaInicio = new DateTime(2025, 8, 1), FechaFin = new DateTime(2026, 2, 1) },
                        new Suscripcion { Id = 4, Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 2, 5), FechaFin = new DateTime(2026, 8, 5) },
                        new Suscripcion { Id = 5, Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 8, 6), FechaFin = new DateTime(2027, 2, 6) }
                    }
                },
                new Comercio
                {
                    Id = 3,
                    Nombre = "Verduleria Las Heras",
                    Rubro = "Verduleria",
                    Direccion = "Las Heras 510",
                    Telefono = "11-4567-3344",
                    Titular = new Titular
                    {
                        Id = 3,
                        Nombre = "Sofia",
                        Apellido = "Diaz",
                        Email = "sofia.diaz@email.com",
                        Telefono = "11-3000-3344"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Id = 6, Plan = "Estandar", Estado = "Activa", FechaInicio = new DateTime(2026, 3, 1), FechaFin = new DateTime(2026, 9, 1) },
                        new Suscripcion { Id = 7, Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 9, 2), FechaFin = new DateTime(2027, 3, 2) }
                    }
                },
                new Comercio
                {
                    Id = 4,
                    Nombre = "Cafe Barrio Norte",
                    Rubro = "Cafeteria",
                    Direccion = "Mitre 980",
                    Telefono = "11-4567-4455",
                    Titular = new Titular
                    {
                        Id = 4,
                        Nombre = "Nicolas",
                        Apellido = "Ruiz",
                        Email = "nicolas.ruiz@email.com",
                        Telefono = "11-3000-4455"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Id = 8, Plan = "Pro", Estado = "Activa", FechaInicio = new DateTime(2026, 1, 20), FechaFin = new DateTime(2026, 7, 20) },
                        new Suscripcion { Id = 9, Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 7, 21), FechaFin = new DateTime(2027, 1, 21) }
                    }
                },
                new Comercio
                {
                    Id = 5,
                    Nombre = "Restaurante La Esquina",
                    Rubro = "Restaurante",
                    Direccion = "Belgrano 1425",
                    Telefono = "11-4567-5566",
                    Titular = new Titular
                    {
                        Id = 5,
                        Nombre = "Carolina",
                        Apellido = "Lopez",
                        Email = "carolina.lopez@email.com",
                        Telefono = "11-3000-5566"
                    },
                    Suscripciones = new List<Suscripcion>
                    {
                        new Suscripcion { Id = 10, Plan = "Estandar", Estado = "Vencida", FechaInicio = new DateTime(2025, 10, 15), FechaFin = new DateTime(2026, 4, 15) },
                        new Suscripcion { Id = 11, Plan = "Pro", Estado = "Activa", FechaInicio = new DateTime(2026, 4, 20), FechaFin = new DateTime(2026, 10, 20) },
                        new Suscripcion { Id = 12, Plan = "Pro", Estado = "Pendiente", FechaInicio = new DateTime(2026, 10, 21), FechaFin = new DateTime(2027, 4, 21) }
                    }
                }
            };
        }
    }
}
