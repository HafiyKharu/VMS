using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment.Dashboard
{
    public class VMSDashboardAppoitnmentLookupTableDto
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public int RegisterAppointment { get; set; }

        public int CheckInAppointment { get; set; }

        public int CancelAppointment { get; set; }

        public int MissAppointment { get; set; }

        public int TodayAppointment { get; set; }

        public int TotalAppointment { get; set; }

        public int CheckOutAppointment { get; set; }

        public int OpenAppointmentTotal { get; set; }

        public int OpenAppointment { get; set; }

        public int OpenAppointmentComplete { get; set; }

        public string Role { get; set; }
    }
}
