using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System.Collections.Generic;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.Storage;
using Visitor.Tower.Dtos;

namespace Visitor.Tower.Exporting
{
    public class TowerExcelExporter : NpoiExcelExporterBase , ITowerExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public TowerExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<GetTowerForViewDto> towers)
        {
            return CreateExcelPackage(
                "Tower.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Towers"));

                    AddHeader(
                        sheet,
                        L("Tower"));
                    AddObjects(
                        sheet, towers,
                        _ => _.Tower.TowerBankRakyat);

                    for (var i = 0; i <= towers.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
