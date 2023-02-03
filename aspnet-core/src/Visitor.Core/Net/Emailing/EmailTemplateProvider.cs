using System;
using System.Collections.Concurrent;
using System.Text;
using Abp.Dependency;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.MultiTenancy;
using Abp.Reflection.Extensions;
using Visitor.MultiTenancy;
using Visitor.Url;

namespace Visitor.Net.Emailing
{
    public class EmailTemplateProvider : IEmailTemplateProvider, ISingletonDependency
    {
        private readonly IWebUrlService _webUrlService;
        private readonly ITenantCache _tenantCache;
        private readonly ConcurrentDictionary<string, string> _defaultTemplates;

        public EmailTemplateProvider(IWebUrlService webUrlService, ITenantCache tenantCache)
        {
            _webUrlService = webUrlService;
            _tenantCache = tenantCache;
            _defaultTemplates = new ConcurrentDictionary<string, string>();
        }

        public string GetDefaultTemplate(int? tenantId)
        {
            var tenancyKey = tenantId.HasValue ? tenantId.Value.ToString() : "host";

            return _defaultTemplates.GetOrAdd(tenancyKey, key =>
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("Visitor.Net.Emailing.EmailTemplates.default.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                    return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                }
            });
        }
        public string GetAppointmentEmailTemplate(int? tenantId)
        {
            var tenancyKey = tenantId.HasValue ? tenantId.Value.ToString() : "host";

            return _defaultTemplates.GetOrAdd(tenancyKey, key =>
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("Visitor.Net.Emailing.EmailTemplates.appointmentEmail.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                    return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                }
            });
        }
        public string GetConfirmCancelEmailTemplate(int? tenantId)
        {
            var tenancyKey = tenantId.HasValue ? tenantId.Value.ToString() : "host";
            
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("Visitor.Net.Emailing.EmailTemplates.confirmCancelAppointment.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                    return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                }
        }

        /*public string GetCancelAppointmentEmailTemplate(int? tenantId)
        {
            var tenancyKey = tenantId.HasValue ? tenantId.Value.ToString() : "host";

            return _defaultTemplates.GetOrAdd(tenancyKey, key =>
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("Visitor.Net.Emailing.EmailTemplates.cancelEmail.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                    return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                }
            });
        }*/
        public string GetThanksEmailTemplate(int? tenantId)
        {
            var tenancyKey = tenantId.HasValue ? tenantId.Value.ToString() : "host";

            return _defaultTemplates.GetOrAdd(tenancyKey, key =>
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("Visitor.Net.Emailing.EmailTemplates.thankEmail.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    template = template.Replace("{THIS_YEAR}", DateTime.Now.Year.ToString());
                    return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                }
            });
        }

        private string GetTenantLogoUrl(int? tenantId)
        {
            if (!tenantId.HasValue)
            {
                return _webUrlService.GetServerRootAddress().EnsureEndsWith('/') + "TenantCustomization/GetTenantLogo/light/png";
            }

            var tenant = _tenantCache.Get(tenantId.Value);
            return _webUrlService.GetServerRootAddress(tenant.TenancyName).EnsureEndsWith('/') + "TenantCustomization/GetTenantLogo/light/" + tenantId.Value + "/png";
        }
    }
}
