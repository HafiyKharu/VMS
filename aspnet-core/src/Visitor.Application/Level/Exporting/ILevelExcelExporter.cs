using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Dto;
using Visitor.Level.Dtos;
using Visitor.PurposeOfVisit.Dtos;

namespace Visitor.Level.Exporting
{
    public interface ILevelExcelExporter
    {
        FileDto ExportToFile(List<GetLevelForViewDto> levels);
    }
}
