using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment.Dashboard
{
    public class CreateOrEditVMSDashboardDto : EntityDto<int?>
    {
        public Guid? AppointmentId { get; set; }
    }
}
