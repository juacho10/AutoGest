using AutoGest.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoGest.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            // ==================== ADMIN ====================
            if (!await context.Usuarios.AnyAsync(u => u.Rol == "Administrador"))
            {
                Console.WriteLine("🔧 Creando usuario administrador...");
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Admin",
                    Apellido = "Sistema",
                    Email = "admin@autogest.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Rol = "Administrador",
                    Activo = true
                });
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Admin: admin@autogest.com / admin123");
            }

            // ==================== EMPLEADO / MECÁNICO ====================
            if (!await context.Usuarios.AnyAsync(u => u.Rol == "Empleado"))
            {
                Console.WriteLine("🔧 Creando empleado/mecánico...");
                context.Usuarios.Add(new Usuario
                {
                    Nombre = "Pedro",
                    Apellido = "Mecánico",
                    Email = "pedro@autogest.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("empleado123"),
                    Rol = "Empleado",
                    Activo = true
                });
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Empleado: pedro@autogest.com / empleado123");
            }

            // ==================== CLIENTES ====================
            if (!await context.Clientes.AnyAsync())
            {
                Console.WriteLine("🔧 Creando clientes de prueba...");
                var clientes = new List<Cliente>
                {
                    new Cliente
                    {
                        Dni = "30123456",
                        Nombre = "Juan",
                        Apellido = "Pérez",
                        Telefono = "1151234567",
                        Email = "juan.perez@email.com",
                        Direccion = "Av. Rivadavia 1234",
                        Activo = true
                    },
                    new Cliente
                    {
                        Dni = "30234567",
                        Nombre = "María",
                        Apellido = "Gómez",
                        Telefono = "1152345678",
                        Email = "maria.gomez@email.com",
                        Direccion = "Belgrano 567",
                        Activo = true
                    },
                    new Cliente
                    {
                        Dni = "30345678",
                        Nombre = "Carlos",
                        Apellido = "López",
                        Telefono = "1153456789",
                        Email = "carlos.lopez@email.com",
                        Direccion = "San Martín 890",
                        Activo = true
                    }
                };
                context.Clientes.AddRange(clientes);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {clientes.Count} clientes creados");
            }

            // ==================== VEHÍCULOS ====================
            if (!await context.Vehiculos.AnyAsync())
            {
                Console.WriteLine("🔧 Creando vehículos de prueba...");
                var cliente1 = await context.Clientes.FirstAsync(c => c.Dni == "30123456");
                var cliente2 = await context.Clientes.FirstAsync(c => c.Dni == "30234567");
                var cliente3 = await context.Clientes.FirstAsync(c => c.Dni == "30345678");

                var vehiculos = new List<Vehiculo>
                {
                    new Vehiculo
                    {
                        Patente = "AB123CD",
                        Marca = "Ford",
                        Modelo = "Focus",
                        Anio = 2018,
                        Kilometraje = 85000,
                        Color = "Gris",
                        ClienteId = cliente1.Id,
                        Activo = true
                    },
                    new Vehiculo
                    {
                        Patente = "EF456GH",
                        Marca = "Chevrolet",
                        Modelo = "Onix",
                        Anio = 2020,
                        Kilometraje = 45000,
                        Color = "Blanco",
                        ClienteId = cliente2.Id,
                        Activo = true
                    },
                    new Vehiculo
                    {
                        Patente = "IJ789KL",
                        Marca = "Volkswagen",
                        Modelo = "Amarok",
                        Anio = 2019,
                        Kilometraje = 120000,
                        Color = "Negro",
                        ClienteId = cliente3.Id,
                        Activo = true
                    }
                };
                context.Vehiculos.AddRange(vehiculos);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {vehiculos.Count} vehículos creados");
            }

            // ==================== REPUESTOS ====================
            if (!await context.Repuestos.AnyAsync())
            {
                Console.WriteLine("🔧 Creando repuestos de prueba...");
                var repuestos = new List<Repuesto>
                {
                    new Repuesto { Codigo = "FIL-001", Descripcion = "Filtro de aceite", Stock = 25, Precio = 3500, Proveedor = "Bosch" },
                    new Repuesto { Codigo = "FIL-002", Descripcion = "Filtro de aire", Stock = 15, Precio = 4200, Proveedor = "Bosch" },
                    new Repuesto { Codigo = "ACE-001", Descripcion = "Aceite 5W30 (1L)", Stock = 40, Precio = 8500, Proveedor = "YPF" },
                    new Repuesto { Codigo = "BAT-001", Descripcion = "Batería 12V 60Ah", Stock = 8, Precio = 85000, Proveedor = "Willard" },
                    new Repuesto { Codigo = "PAS-001", Descripcion = "Pastillas de freno delanteras", Stock = 12, Precio = 25000, Proveedor = "Brembo" },
                    new Repuesto { Codigo = "BOM-001", Descripcion = "Bomba de agua", Stock = 5, Precio = 45000, Proveedor = "SKF" },
                    new Repuesto { Codigo = "COR-001", Descripcion = "Correa de distribución", Stock = 10, Precio = 32000, Proveedor = "Gates" },
                    new Repuesto { Codigo = "Buj-001", Descripcion = "Bujía de encendido", Stock = 50, Precio = 2800, Proveedor = "NGK" }
                };
                context.Repuestos.AddRange(repuestos);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {repuestos.Count} repuestos creados");
            }

            Console.WriteLine("🎉 Base de datos inicializada con datos de prueba");
        }
    }
}