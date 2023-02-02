using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Dto;
using Visitor.Title.Dtos;

namespace Visitor.Appointment.Exporting
{
    public interface ITitleExcelExporter
    {
        FileDto ExportToFile(List<GetTitleForViewDto> titles);
    }
}
