using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.PurposeOfVisit.Dtos;
using Visitor.Storage;

namespace Visitor.PurposeOfVisit.Exporting
{
    public class PurposeOfVisitExcelExporter : NpoiExcelExporterBase , IPurposeOfVisitExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PurposeOfVisitExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<GetPurposeOfVisitForViewDto> purposeOfVisits)
        {
            return CreateExcelPackage(
                "PurposeOfVisit.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("PurposeOfVisits"));

                    AddHeader(
                        sheet,
                        L("PurposeOfVisit"));
                    AddObjects(
                        sheet, purposeOfVisits,
                        _ => _.PurposeOfVisit.PurposeOfVisitApp);

                    for (var i = 0; i <= purposeOfVisits.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
