using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.Level.Dtos;
using Visitor.PurposeOfVisit.Dtos;
using Visitor.Storage;

namespace Visitor.Level.Exporting
{
    public class LevelExcelExporter : NpoiExcelExporterBase , ILevelExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public LevelExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<GetLevelForViewDto> levels)
        {
            return CreateExcelPackage(
                "Level.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Levels"));

                    AddHeader(
                        sheet,
                        L("Level"));
                    AddObjects(
                        sheet, levels,
                        _ => _.Level.LevelBankRakyat);

                    for (var i = 0; i <= levels.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
