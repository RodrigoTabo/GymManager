namespace GymManager.Api.Domain.Entities
{
    public class Pago : ICurrentSucursalService
    {
        public int Id { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }
        public DateTime CubreDesde { get; set; }
        public DateTime CubreHasta { get; set; }
        public int MetodoPagoId { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; }
        public DateTime? EliminadoEn { get; set; }

        public Guid? SucursalId { get; set; }
    }
}
