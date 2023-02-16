using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment.Dashboard
{
    public class VMSDashboardAppoitnmentLookupTableDto
    {
        public int TotalToday { get; set; }
        public int Register { get; set; }
        public int CheckIn { get; set; }
        public int Cancel { get; set; }
        public int CheckOut { get; set; }
    }
}
