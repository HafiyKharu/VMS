using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Departments.Dtos;
using Visitor.Dto;
using Visitor.Level.Dtos;
using Visitor.Storage;

namespace Visitor.Departments.Exporting
{
    public class DepartmentExcelExporter : NpoiExcelExporterBase , IDepartmentExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public DepartmentExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<GetDepartmentForViewDto> departments)
        {
            return CreateExcelPackage(
                "Department.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Departments"));

                    AddHeader(
                        sheet,
                        L("Department"));
                    AddObjects(
                        sheet, departments,
                        _ => _.Department.DepartmentName);

                    for (var i = 0; i <= departments.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
