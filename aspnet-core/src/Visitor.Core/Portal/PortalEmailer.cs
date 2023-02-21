using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.UI;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Visitor.Appointment;
using Visitor.Configuration;
using Visitor.Net.Emailing;

namespace Visitor.core.Portal
{
    public class PortalEmailer: VisitorServiceBase , IPortalEmailer , ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IEmailSender _emailSender;
        private readonly IAppConfigurationAccessor _configurationAccessor;

        public PortalEmailer(
            ISettingManager settingManager,
            IEmailTemplateProvider emailTemplateProvider,
            IEmailSender emailSender,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _settingManager = settingManager;
            _emailTemplateProvider = emailTemplateProvider;
            _emailSender = emailSender;
            _configurationAccessor = configurationAccessor;
        }

        public async Task SendEmailDetailAppointmentAsync(AppointmentEnt appointment)
        {
            await CheckMailSettingsEmptyOrNull();

            var dateTimeInfo = new StringBuilder();
            dateTimeInfo.AppendLine(L("AppDateTime")+":" + "<br>" );
            dateTimeInfo.AppendLine(appointment.AppDateTime.ToString("dddd, dd MMMM yyyy ") + " at " + appointment.AppDateTime.ToString("hh:mm tt"));

            //email visitor
            var emailTemplateVisitor = GetTitleAndSubTitle(null, appointment.AppRefNo, dateTimeInfo);
            //email officer to meet
            var emailTemplateOfficerToMeet = GetTitleAndSubTitle(null, appointment.AppRefNo, dateTimeInfo);
            //context email
            var VisitorDetails = new StringBuilder();
            var AppointmentDetails = new StringBuilder();
            var CancelDetails = new StringBuilder();
            var Regard = new StringBuilder();
            //cancel link
            var surveyUrl = _configurationAccessor.Configuration["Survey:surveyUrl"] + "cancel?appointmentId=" + appointment.Id + "&&Item=Cancel";

            //visitor
            VisitorDetails.AppendLine("<td style='border-bottom: 1px solid #dee2e6 ; color: #222; padding: 10px 0px;'>");
            VisitorDetails.AppendLine(L("VisitorDetails") + ":" );
            VisitorDetails.AppendLine("</td>");
            VisitorDetails.AppendLine("<td style='border-bottom: 1px solid #dee2e6 ; color: #222; padding: 10px 0px;'>");
            VisitorDetails.AppendLine(L("VisitorFullName") + " : " + appointment.Title + " " + appointment.FullName + "<br>" + L("IC/Pas") + " : " + appointment.IdentityCard + "<br>" + L("PhoneNo") + " : " + appointment.PhoneNo + "<br>" + L("VisitorEmail") + " : " + appointment.Email);
            VisitorDetails.AppendLine("</td>");
            VisitorDetails.AppendLine("<br>");

            //appointment
            AppointmentDetails.AppendLine("<td style='border-bottom: 1px solid #dee2e6 ; color: #222; padding: 10px 0px;'>");
            AppointmentDetails.AppendLine(L("AppointmentDetails") + ":");
            AppointmentDetails.AppendLine("</td>");
            AppointmentDetails.AppendLine("<td style='border-bottom: 1px solid #dee2e6 ; color: #222; padding: 10px 0px;'>");
            AppointmentDetails.AppendLine(L("Tower") + " : " + appointment.Tower + "<br>" + L("Company") + " : " + appointment.CompanyName + "<br>" + L("Department") + " : " + appointment.Department + "<br>"
            + L("Level") + " : " + appointment.Level + "<br>" + L("OfficerToMeet") + " : " + appointment.OfficerToMeet + "<br>" + L("EmailOfficerToMeet") + " : " + appointment.EmailOfficerToMeet
            + "<br>" + L("PhoneNoOfficerToMeet") + " : " + appointment.PhoneNoOfficerToMeet + "<br>" + L("PurposeOfVisit") + " : " + appointment.PurposeOfVisit);
            AppointmentDetails.AppendLine("</td>");

            //cancel appointment
            CancelDetails.AppendLine("<td style='border-bottom: 1px solid #dee2e6 ; color: #222; padding: 10px 0px;'>");
            CancelDetails.AppendLine(L("CancelEmailBodyContent2"));
            CancelDetails.AppendLine("<br>");
            CancelDetails.AppendLine(String.Format("<a href='" + surveyUrl + "'>Cancel</a>"));
            CancelDetails.AppendLine("<br><br>" + L("CancelEmailBodyContent3"));
            CancelDetails.AppendLine("</td>");

            //Regard
            Regard.AppendLine(L("Sincerely"));
            Regard.AppendLine("<br>");
            Regard.AppendLine(L("BankRakyat"));

            //From here on, you can implement your platform-dependent byte[]-to-image code 

            //e.g. Windows 10 - Full .NET Framework


            await ReplaceBodyAndSend(appointment.Email, L("AppointmentDetails"), emailTemplateVisitor, VisitorDetails, AppointmentDetails, CancelDetails , Regard);
            await ReplaceBodyAndSend(appointment.EmailOfficerToMeet, L("AppointmentDetails"), emailTemplateOfficerToMeet, VisitorDetails, AppointmentDetails, CancelDetails, Regard);

        }
        private StringBuilder GetTitleAndSubTitle(int? tenantId, string title, StringBuilder dateTimeInfo)
        {
            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetAppointmentEmailTemplate(tenantId));
            emailTemplate.Replace("{EMAIL_TITLE}", title);
            emailTemplate.Replace("{EMAIL_SUB_TITLE}", dateTimeInfo.ToString());

            return emailTemplate;
        }
        private async Task ReplaceBodyAndSend(string emailAddress, string subject, StringBuilder emailTemplate, 
            StringBuilder VisitorDetails,StringBuilder AppointmentDetails,StringBuilder CancelDetails, StringBuilder Regard)
        {
            emailTemplate.Replace("{VISITOR_INFO}", VisitorDetails.ToString());
            emailTemplate.Replace("{APPOINTMENT_INFO}", AppointmentDetails.ToString());
            emailTemplate.Replace("{CANCEL_INFO}", CancelDetails.ToString());
            emailTemplate.Replace("{REGARD_INFO}", Regard.ToString());

            await _emailSender.SendAsync(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true,
            });
            }
        private async Task ReplaceBodyAndSendCancel(string emailAddress, string subject, StringBuilder emailTemplate, StringBuilder mailMessage)
        {
            emailTemplate.Replace("{EMAIL_BODY}", mailMessage.ToString());

            await _emailSender.SendAsync(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true,
            });
        }
        public async Task SendConfirmCancelEmailAsync(AppointmentEnt appointment)
        {
            DateTime DTNow = DateTime.Now;
            await CheckMailSettingsEmptyOrNull();
            var emailTemplate = GetTitleAndSubTitleConfirmCancel(null, appointment.AppRefNo);
            var greeting = new StringBuilder();
            var cancelInfo = new StringBuilder();
            var regardInfo = new StringBuilder();

            greeting.AppendLine(L("CancelEmailIntro") + appointment.Title + " " + appointment.FullName + " /  " + appointment.OfficerToMeet);

            cancelInfo.AppendLine("<br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1") + DTNow.ToString("dd/MM/yyyy hh:mm:tt"));
            cancelInfo.AppendLine("<br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1.2"));
            cancelInfo.AppendLine("<br><br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1.3"));
            cancelInfo.AppendLine("<br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1.4"));
            cancelInfo.AppendLine("<br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1.5"));
            cancelInfo.AppendLine("<br>");
            cancelInfo.AppendLine(L("CancelEmailBodyContent1.6"));
            cancelInfo.AppendLine("<br>");

            //Regard
            regardInfo.AppendLine("<br>");
            regardInfo.AppendLine(L("Sincerely"));
            regardInfo.AppendLine("<br>");
            regardInfo.AppendLine(L("BankRakyat"));

            //From here on, you can implement your platform-dependent byte[]-to-image code 

            await ReplaceBodyAndSendCancel(appointment.Email, L("CancelEmailBodyTitle"), emailTemplate, greeting, cancelInfo, regardInfo);
            await ReplaceBodyAndSendCancel(appointment.EmailOfficerToMeet, L("CancelEmailBodyTitle"), emailTemplate, greeting, cancelInfo, regardInfo);
        }
        private StringBuilder GetTitleAndSubTitleConfirmCancel(int? tenantId, string title)
        {
            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetConfirmCancelEmailTemplate(tenantId));
            emailTemplate.Replace("{EMAIL_TITLE}", title);

            return emailTemplate;
        }
        private async Task ReplaceBodyAndSendCancel(
            string emailAddress, string subject, StringBuilder emailTemplate,
            StringBuilder greeting, StringBuilder cancelInfo, StringBuilder regardInfo)
        {
            emailTemplate.Replace("{GREETING}", greeting.ToString());
            emailTemplate.Replace("{CANCEL_INFO}", cancelInfo.ToString());
            emailTemplate.Replace("{REGARD_INFO}", regardInfo.ToString());

            await _emailSender.SendAsync(new MailMessage
            {
                To = { emailAddress },
                Subject = subject,
                Body = emailTemplate.ToString(),
                IsBodyHtml = true,
            });
        }
        private async Task CheckMailSettingsEmptyOrNull()
        {
#if DEBUG
            return;
#endif
            if (
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress)).IsNullOrEmpty() ||
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host)).IsNullOrEmpty()
            )
            {
                throw new UserFriendlyException(L("SMTPSettingsNotProvidedWarningText"));
            }

            if ((await _settingManager.GetSettingValueAsync<bool>(EmailSettingNames.Smtp.UseDefaultCredentials)))
            {
                return;
            }

            if (
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName)).IsNullOrEmpty() ||
                (await _settingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password)).IsNullOrEmpty()
            )
            {
                throw new UserFriendlyException(L("SMTPSettingsNotProvidedWarningText"));
            }
        }
    }
}
