using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Departments.Dtos;
using Visitor.Dto;

namespace Visitor.Departments.Exporting
{
    public interface IDepartmentExcelExporter
    {
        FileDto ExportToFile(List<GetDepartmentForViewDto> departments);
    }
}
