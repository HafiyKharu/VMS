using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Blacklist.Dtos
{
    public class GetAllBlacklistForExcelInput
    {
        public string Filter { get; set; }
        public string FullNameFilter { get; set; }
    }
}
