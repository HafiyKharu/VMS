using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Blacklist.Dtos;
using Visitor.Company.Dtos;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.Storage;

namespace Visitor.Blacklist.Exporting
{
    public class BlacklistExcelExporter : NpoiExcelExporterBase , IBlacklistExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public BlacklistExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetBlacklistForViewDto> blacklists)
        {
            return CreateExcelPackage(
                "Blacklists.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Blacklists"));

                    AddHeader(
                        sheet,
                        L("BlacklistFullName"),
                        L("BlacklistIdentityCard"),
                        L("BlacklistPhoneNumber"),
                        L("BlacklistRemarks")
                        );

                    AddObjects(
                        sheet, blacklists,
                        _ => _.Blacklist.BlacklistFullName,
                        _ => _.Blacklist.BlacklistIdentityCard,
                        _ => _.Blacklist.BlacklistPhoneNumber,
                        _ => _.Blacklist.BlacklistRemarks
                        );
                    for (var i = 0; i <= 4; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
