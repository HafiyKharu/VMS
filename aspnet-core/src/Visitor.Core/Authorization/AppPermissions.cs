namespace Visitor.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        //TOWER
        public const string Pages_Towers = "Pages.Towers";
        public const string Pages_Towers_Create = "Pages.Towers.Create";
        public const string Pages_Towers_Edit = "Pages.Towers.Edit";
        public const string Pages_Towers_Delete = "Pages.Towers.Delete";

        //PURPOSE OF VISIT
        public const string Pages_PurposeOfVisits = "Pages.PurposeOfVisits";
        public const string Pages_PurposeOfVisits_Create = "Pages.PurposeOfVisits.Create";
        public const string Pages_PurposeOfVisits_Edit = "Pages.PurposeOfVisits.Edit";
        public const string Pages_PurposeOfVisits_Delete = "Pages.PurposeOfVisits.Delete";

        //VISITOR TITLE
        public const string Pages_Titles= "Pages.Titles";
        public const string Pages_Titles_Create = "Pages.Titles.Create";
        public const string Pages_Titles_Edit = "Pages.Titles.Edit";
        public const string Pages_Titles_Delete = "Pages.Titles.Delete";

        //VISITOR LEVEL
        public const string Pages_Levels = "Pages.Levels";
        public const string Pages_Levels_Create = "Pages.Levels.Create";
        public const string Pages_Levels_Edit = "Pages.Levels.Edit";
        public const string Pages_Levels_Delete = "Pages.Levels.Delete";

        //Company
        public const string Pages_Companies = "Pages.Companies";
        public const string Pages_Companies_Create = "Pages.Companies.Create";
        public const string Pages_Companies_Edit = "Pages.Companies.Edit"; 
        public const string Pages_Companies_Delete = "Pages.Companies.Delete";

        // PERMISSION FOR APPOINTMENTS

        public const string Pages_Appointments = "Pages.Appointments";
        public const string Pages_Appointments_Create = "Pages.Appointments.Create";
        public const string Pages_Appointments_Edit = "Pages.Appointments.Edit";
        public const string Pages_Appointments_Delete = "Pages.Appointments.Delete";

        // PERMISSION FOR DEPARTMENTS

        public const string Pages_Departments = "Pages.Departments";
        public const string Pages_Departments_Create = "Pages.Departments.Create";
        public const string Pages_Departments_Edit = "Pages.Departments.Edit";
        public const string Pages_Departments_Delete = "Pages.Departments.Delete";

        // PERMISSION FOR EXPIRED URL

        public const string Pages_ExpiredUrls = "Pages.ExpiredUrls";
        public const string Pages_ExpiredUrls_Create = "Pages.ExpiredUrls.Create";
        public const string Pages_ExpiredUrls_Edit = "Pages.ExpiredUrls.Edit";
        public const string Pages_ExpiredUrls_Delete = "Pages.ExpiredUrls.Delete";

        // PERMISSION FOR VMS Dashboard

        public const string Pages_VMSDashboards = "Pages.VMSDashboards";
        public const string Pages_VMSDashboards_Create = "Pages.VMSDashboards.Create";
        public const string Pages_VMSDashboards_Edit = "Pages.VMSDashboards.Edit";
        public const string Pages_VMSDashboards_Delete = "Pages.VMSDashboards.Delete";

        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

        public const string Pages = "Pages";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        //BLACLIST
        public const string Pages_Blacklists = "Pages.Blacklists";
        public const string Pages_Blacklists_Create = "Pages.Blacklists.Create";
        public const string Pages_Blacklists_Edit = "Pages.Blacklists.Edit";
        public const string Pages_Blacklists_Delete = "Pages.Blacklists.Delete";
        //

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";
        public const string Pages_Administration_Users_ChangeProfilePicture = "Pages.Administration.Users.ChangeProfilePicture";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";
        public const string Pages_Administration_Languages_ChangeDefaultLanguage = "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";
        public const string Pages_Administration_OrganizationUnits_ManageRoles = "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_UiCustomization = "Pages.Administration.UiCustomization";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";
        public const string Pages_Administration_WebhookSubscription_Create = "Pages.Administration.WebhookSubscription.Create";
        public const string Pages_Administration_WebhookSubscription_Edit = "Pages.Administration.WebhookSubscription.Edit";
        public const string Pages_Administration_WebhookSubscription_ChangeActivity = "Pages.Administration.WebhookSubscription.ChangeActivity";
        public const string Pages_Administration_WebhookSubscription_Detail = "Pages.Administration.WebhookSubscription.Detail";
        public const string Pages_Administration_Webhook_ListSendAttempts = "Pages.Administration.Webhook.ListSendAttempts";
        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";
        public const string Pages_Administration_DynamicProperties_Create = "Pages.Administration.DynamicProperties.Create";
        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";
        public const string Pages_Administration_DynamicProperties_Delete = "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";
        public const string Pages_Administration_DynamicPropertyValue_Create = "Pages.Administration.DynamicPropertyValue.Create";
        public const string Pages_Administration_DynamicPropertyValue_Edit = "Pages.Administration.DynamicPropertyValue.Edit";
        public const string Pages_Administration_DynamicPropertyValue_Delete = "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties = "Pages.Administration.DynamicEntityProperties";
        public const string Pages_Administration_DynamicEntityProperties_Create = "Pages.Administration.DynamicEntityProperties.Create";
        public const string Pages_Administration_DynamicEntityProperties_Edit = "Pages.Administration.DynamicEntityProperties.Edit";
        public const string Pages_Administration_DynamicEntityProperties_Delete = "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue = "Pages.Administration.DynamicEntityPropertyValue";
        public const string Pages_Administration_DynamicEntityPropertyValue_Create = "Pages.Administration.DynamicEntityPropertyValue.Create";
        public const string Pages_Administration_DynamicEntityPropertyValue_Edit = "Pages.Administration.DynamicEntityPropertyValue.Edit";
        public const string Pages_Administration_DynamicEntityPropertyValue_Delete = "Pages.Administration.DynamicEntityPropertyValue.Delete";

        public const string Pages_Administration_MassNotification = "Pages.Administration.MassNotification";
        public const string Pages_Administration_MassNotification_Create = "Pages.Administration.MassNotification.Create";

        //TENANT-SPECIFIC PERMISSIONS

        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";



    }
}