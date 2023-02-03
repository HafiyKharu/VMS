using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment
{
    public class GetAllAppointmentForExcelInput
    {
        public string Filter { get; set; }
        public string FullNameFilter { get; set; }
        //public DateTime Masa { get; set; }

        public string IdentityCardFilter { get; set; }
        public string PhoneNoFilter { get; set; }
        public string EmailFilter { get; set; }
        public string TitleFilter { get; set; }
        public string CompanyNameFilter { get; set; }
        public string OfficerToMeetFilter { get; set; }
        public string PurposeOfVisitFilter { get; set; }
        public string DepartmentFilter { get; set; }
        public string TowerFilter { get; set; }
        public string LevelFilter { get; set; }
        public DateTime? MinAppDateTimeFilter { get; set; }
        public DateTime? MaxAppDateTimeFilter { get; set; }
        public DateTime? MinRegDateTimeFilter { get; set; }
        public DateTime? MaxRegDateTimeFilter { get; set; }

        public string EmailOfficerToMeet { get; set; }
        public string PhoneNoOfficerToMeet { get; set; }

        public int? StatusFilter { get; set; }

        public string PassNumberFilter { get; set; }

        public string AppRefNoFilter { get; set; }
    }
}
