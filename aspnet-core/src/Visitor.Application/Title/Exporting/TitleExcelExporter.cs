using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System.Collections.Generic;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.Storage;
using Visitor.Title.Dtos;

namespace Visitor.Title.Exporting
{
    public class TitleExcelExporter : NpoiExcelExporterBase, ITitleExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public TitleExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<GetTitleForViewDto> titles)
        {
            return CreateExcelPackage(
                "Title.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Titles"));

                    AddHeader(
                        sheet,
                        L("Title"));
                    AddObjects(
                        sheet, titles,
                        _ => _.Title.VisitorTitle);

                    for (var i = 0; i <= titles.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
