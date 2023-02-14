using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Appointment.Dashboard
{
    public class GetAllVMSDashboardInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string FullName { get; set; }
    }
}
