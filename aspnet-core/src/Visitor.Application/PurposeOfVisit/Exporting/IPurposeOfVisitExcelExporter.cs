using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Dto;
using Visitor.PurposeOfVisit.Dtos;

namespace Visitor.PurposeOfVisit.Exporting
{
    public interface IPurposeOfVisitExcelExporter
    {
        FileDto ExportToFile(List<GetPurposeOfVisitForViewDto> purposeOfVisits);
    }
}
