using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visitor.Appointment.Dashboard
{
    [Table("VMSDashboards")]
    public class VMSDashboardEnt : Entity , IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public virtual Guid? AppoitnmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public AppointmentEnt AppointmentFk { get; set; }
    }
}
