using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Company.Dtos;
using Visitor.Dto;
using Visitor.Storage;
using Visitor.Departments;

namespace Visitor.Company.Exporting
{
    public class CompaniesExcelExporter : NpoiExcelExporterBase, ICompaniesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CompaniesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetCompanyForViewDto> companies)
        {
            return CreateExcelPackage(
                "Companies.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Companies"));

                    AddHeader(
                        sheet,
                        L("CompanyName"),
                        L("CompanyEmail"),
                        L("OfficePhoneNumber"),
                        L("CompanyAddress")
                        );

                    AddObjects(
                        sheet, companies,
                        _ => _.Company.CompanyName,
                        _ => _.Company.CompanyEmail,
                        _ => _.Company.OfficePhoneNumber,
                        _ => _.Company.CompanyAddress
                        );
                    for (var i = 0; i <= 4; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}