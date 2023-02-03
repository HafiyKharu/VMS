using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using System.Collections.Generic;
using Visitor.DataExporting.Excel.NPOI;
using Visitor.Dto;
using Visitor.Storage;

namespace Visitor.Appointment.Exporting
{
    public class AppointmentExcelExporter : NpoiExcelExporterBase, IAppointmentExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public AppointmentExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetAppointmentForViewDto> appointments)
        {
            return CreateExcelPackage(
                "Appointments.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Appointments"));

                    AddHeader(
                        sheet,
                        L("Title"),
                        L("FullName"),
                        L("IC"),
                        L("PhoneNumber"),
                        L("EmailAddress"),
                        L("AppDateTime"),
                        L("Tower"),
                        L("CompanyName"),
                        L("Level"),
                        L("OfficerToMeet"),
                        L("EmailOfficerToMeet"),                        
                        L("PhoneNoOfficerToMeet"),
                        L("PurposeOfVisit"),
                        L("DepartmentName"),
                        L("RefNo"),
                        L("PassNumber"),
                        L("Status"),
                        L("RegDateTime"),
                        L("IdImage"),
                        L("checkInDateTime"),
                        L("checkOutDateTime"),
                        L("cancelDateTime")
                        );

                    AddObjects(
                        sheet, appointments,
                        _ => _.Appointment.Title,
                        _ => _.Appointment.FullName,
                        _ => _.Appointment.IdentityCard,
                        _ => _.Appointment.PhoneNo,
                        _ => _.Appointment.Email,
                        _ => _.Appointment.AppDateTime,
                        _ => _.Appointment.Tower,
                        _ => _.Appointment.CompanyName,
                        _ => _.Appointment.Level,
                        _ => _.Appointment.OfficerToMeet,
                        _ => _.Appointment.EmailOfficerToMeet,
                        _ => _.Appointment.PhoneNoOfficerToMeet,
                        _ => _.Appointment.PurposeOfVisit,
                        _ => _.Appointment.Department,
                        _ => _.Appointment.AppRefNo,
                        _ => _.Appointment.AppRefNo,
                        _ => _.Appointment.Status,
                        _ => _.Appointment.CreationTime,
                        _ => _.Appointment.ImageId,
                        _ => _.Appointment.CheckInDateTime,
                        _ => _.Appointment.CheckOutDateTime,
                        _ => _.Appointment.CancelDateTime
                        );

                    for (var i = 0; i <= 21; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
