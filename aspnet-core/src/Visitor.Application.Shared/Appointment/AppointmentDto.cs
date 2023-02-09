using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Visitor.Appointments;
using Visitor.Common;

namespace Visitor.Appointment
{
    public class AppointmentDto : FullAuditedEntityDto<Guid>,IHasCreationTime
    { 
        public string IdentityCard { get; set; }
        public string FullName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string OfficerToMeet { get; set; }
        public string PurposeOfVisit { get; set; }
        public string Department { get; set; }
        public string Tower { get; set; }
        public string Level { get; set; }
        public DateTime AppDateTime { get; set; }
        public DateTime RegDateTime { get; set; }
        public StatusType Status { get; set; }
        public string ImageId { get; set; }
        public string AppRefNo { get; set; }
        public string PassNumber { get; set; }
        public DateTime CheckInDateTime { get; set; }
        public DateTime CheckOutDateTime { get; set; }
        public string EmailOfficerToMeet { get; set; }
        public string PhoneNoOfficerToMeet { get; set; }
        public DateTime CancelDateTime { get; set; }
    }
}
