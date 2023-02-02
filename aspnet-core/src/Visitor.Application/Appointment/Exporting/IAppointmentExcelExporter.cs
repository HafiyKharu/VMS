using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Dto;

namespace Visitor.Appointment.Exporting
{
    public interface IAppointmentExcelExporter
    {
        FileDto ExportToFile(List<GetAppointmentForViewDto> appointments);
    }
}
