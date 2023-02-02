using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Dto;
using Visitor.Tower.Dtos;

namespace Visitor.Tower.Exporting
{
    public interface ITowerExcelExporter
    {
        FileDto ExportToFile(List<GetTowerForViewDto> titles);
    }
}
