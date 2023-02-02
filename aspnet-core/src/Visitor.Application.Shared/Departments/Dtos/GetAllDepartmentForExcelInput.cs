using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Departments.Dtos
{
    public class GetAllDepartmentForExcelInput
    {
        public string Filter { get; set; }
        public string DepartmentNameFilter { get; set; }
    }
}
