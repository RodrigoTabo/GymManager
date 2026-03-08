using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Planes
{
    public class StatsPlanRequest
    {
        public int PlanActivos { get; set; }
        public List<PlanContadorResponse> TopPlanes { get; set; } = new();

    }

    public class PlanContadorResponse
    {
        public int PlanId { get; set; }
        public string NombrePlan { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }
}
