using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Blacklist.Dtos;
using Visitor.Dto;

namespace Visitor.Blacklist.Exporting
{
    public interface IBlacklistExcelExporter
    {
        FileDto ExportToFile(List<GetBlacklistForViewDto> blacklists);
    }
}
