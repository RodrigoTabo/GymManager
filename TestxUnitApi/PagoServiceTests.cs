using GymManager.Api.Application.Services;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace TestxUnitApi
{
    public class PagoServiceTests
    {
        private readonly GymManagerDbContext _context;
        private readonly Mock<ICurrentUserService> _mockCurrentUser;
        private readonly PagoService _service;
        private readonly Guid _sucursalId;

        public PagoServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymManagerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new GymManagerDbContext(options);

            _mockCurrentUser = new Mock<ICurrentUserService>();
            _sucursalId = Guid.NewGuid();

            _mockCurrentUser
                .Setup(x => x.SucursalIdOrThrow)
                .Returns(_sucursalId);

            _service = new PagoService(_context, _mockCurrentUser.Object);
        }


        //Creo las antidades para tenerlas cargadas y no repetir
        private async Task SeedDatosBasicos()
        {
            var socio = new Socio
            {
                Id = 1,
                Nombre = "Rodrigo",
                Apellido = "Tabó",
                DNI = "123",
                Telefono = "123",
                SucursalId = _sucursalId,
                PlanId = 1,
                EliminadoEn = null
            };

            var metodoPago = new MetodoPago
            {
                Id = 1,
                Nombre = "Transferencia",
                SucursalId = _sucursalId,
                EliminadoEn = null
            };

            var plan = new Plan
            {
                Id = 1,
                Nombre = "Plan mensual",
                Precio = 1000,
                DuracionDias = 30,
                SucursalId = _sucursalId,
                EliminadoEn = null
            };

            _context.Socios.Add(socio);
            _context.MetodosPago.Add(metodoPago);
            _context.Planes.Add(plan);

            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CrearAsync_DeberiaCrearPago_CuandoDatosSonValidos()
        {
            // Arrange
            await SeedDatosBasicos();

            var request = new CreatePagoRequest(1, 1);

            // Act
            var result = await _service.CrearAsync(request);

            // Assert
            var pago = await _context.Pagos.FirstOrDefaultAsync();

            Assert.NotNull(pago);
            Assert.Equal(1, pago.SocioId);
            Assert.Equal(1000, pago.Importe);
            Assert.Equal(_sucursalId, pago.SucursalId);
            Assert.True(result > 0);
        }

        [Fact]
        public async Task UpdateAsync_DeberiaActualizarPago_CuandoDatosSonValidos()
        {
            // Arrange
            await SeedDatosBasicos();

            var pagoOriginal = new Pago
            {
                Id = 1,
                SocioId = 1,
                MetodoPagoId = 1,
                Importe = 1000,
                SucursalId = _sucursalId,
                FechaPago = DateTime.UtcNow,
                CubreDesde = DateTime.UtcNow,
                CubreHasta = DateTime.UtcNow.AddMonths(1)
            };

            _context.Pagos.Add(pagoOriginal);

            var nuevoMetodoId = 2;

            _context.MetodosPago.Add(new MetodoPago
            {
                Id = nuevoMetodoId,
                Nombre = "Efectivo",
                SucursalId = _sucursalId,
                EliminadoEn = null
            });

            await _context.SaveChangesAsync();

            var request = new UpdatePagoRequest(nuevoMetodoId);

            // Act
            await _service.UpdateAsync(request, pagoOriginal.Id);

            // Assert
            var pagoActualizado = await _context.Pagos.FindAsync(pagoOriginal.Id);

            Assert.NotNull(pagoActualizado);
            Assert.Equal(nuevoMetodoId, pagoActualizado.MetodoPagoId);
        }

        [Fact]
        public async Task UpdateAsync_DeberiaFallar_SiPagoNoExiste()
        {
            // Arrange
            var request = new UpdatePagoRequest(1);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _service.UpdateAsync(request, 999));
        }
    }
}
