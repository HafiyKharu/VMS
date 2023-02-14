using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment.Dashboard
{
    public class VMSDashboardDto : EntityDto
    {
        public Guid? AppointmentId { get; set; }
    }
}
