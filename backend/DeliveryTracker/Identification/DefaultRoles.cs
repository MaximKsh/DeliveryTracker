using System.Collections.Generic;
using DeliveryTracker.Localization;

namespace DeliveryTracker.Identification
{
    public static class DefaultRoles
    {
        public static readonly IReadOnlyList<string> AllRoles = new List<string>
        {
            CreatorRole,
            ManagerRole,
            PerformerRole,
        }.AsReadOnly();
        
        public const string CreatorRole = "CreatorRole";

        public const string ManagerRole = "ManagerRole";

        public const string PerformerRole = "PerformerRole";

        public const string CreatorRoleCaption = LocalizationAlias.Roles.CreatorRole;

        public const string ManagerRoleCaption = LocalizationAlias.Roles.ManagerRole;

        public const string PerformerRoleCaption = LocalizationAlias.Roles.PerformerRole;
        
    }
}