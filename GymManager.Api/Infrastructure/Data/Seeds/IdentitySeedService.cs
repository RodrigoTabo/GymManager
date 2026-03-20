using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymManager.Api.Domain.Entities;

namespace GymManager.Api.Infrastructure.Data.Seeds
{


    public class IdentitySeedService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly GymManagerDbContext _context;

        public IdentitySeedService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            GymManagerDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            const string roleName = "OwnerAdmin";
            const string email = "rodri@hotmail.com";
            const string password = "Admin1234";

            // 1) Crear rol si no existe
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(" | ", roleResult.Errors.Select(x => x.Description));
                    throw new Exception($"Error creando rol {roleName}: {errors}");
                }
            }

            // 2) Crear sucursales base si no existen
            await SeedSucursalesAsync(_context);

            // 3) Buscar sucursal principal para asignar al owner
            var sucursalPrincipal = await _context.Sucursales
                .OrderBy(x => x.Nombre)
                .FirstAsync();

            // 4) Crear usuario owner si no existe
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    Nombre = "Owner",
                    Activo = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(x => x.Description));
                    throw new Exception($"Error creando usuario owner: {errors}");
                }
            }

            // 5) Asignar rol si todavía no lo tiene
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);

                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(" | ", addRoleResult.Errors.Select(x => x.Description));
                    throw new Exception($"Error asignando rol {roleName}: {errors}");
                }
            }

            // 6) Vincular usuario a sucursal principal si no existe la relación
            var existeRelacion = await _context.UsuarioSucursales
                .AnyAsync(x => x.UsuarioId == user.Id && x.SucursalId == sucursalPrincipal.Id);

            if (!existeRelacion)
            {
                _context.UsuarioSucursales.Add(new UsuarioSucursal
                {
                    UsuarioId = user.Id,
                    SucursalId = sucursalPrincipal.Id,
                    EsPrincipal = true
                });

                await _context.SaveChangesAsync();
            }
        }

        private static async Task SeedSucursalesAsync(GymManagerDbContext context)
        {
            if (await context.Sucursales.AnyAsync())
                return;

            context.Sucursales.AddRange(
                new Sucursal
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Central",
                    Activa = true
                },
                new Sucursal
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Sucursal Norte",
                    Activa = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}