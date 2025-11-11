using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikVesipiip.Models
{
    public class PaevaVahetus
    {
        public DateTime Paev { get; set; }
        public bool OnKinni { get; set; }

        // ➜ Часы открытия/закрытия дня (минимальное начало и максимальный конец смен)
        public TimeSpan Algus { get; set; }
        public TimeSpan Lopp { get; set; }

        public List<ShiftSummary> Tootajad { get; set; } = new();
        public List<Gap> Tuhimik { get; set; } = new();

        public class ShiftSummary
        {
            public int VahetusId { get; set; }
            public int TootajaId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Color { get; set; }
            public TimeSpan Algus { get; set; }
            public TimeSpan Lopp { get; set; }
        }

        public class Gap
        {
            public TimeSpan Algus { get; set; }
            public TimeSpan Lopp { get; set; }
        }
    }
}
