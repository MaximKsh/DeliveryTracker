using System.Collections.Generic;
using DeliveryTracker.Localization;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Список стандартных ролей.
    /// </summary>
    public static class DefaultRoles
    {
        /// <summary>
        /// Все стандартные роли
        /// </summary>
        public static readonly IReadOnlyList<string> AllRoles = new List<string>
        {
            CreatorRole,
            ManagerRole,
            PerformerRole,
        }.AsReadOnly();
        
        /// <summary>
        /// Создатель компании.
        /// </summary>
        public const string CreatorRole = "CreatorRole";

        /// <summary>
        /// Менеджер.
        /// </summary>
        public const string ManagerRole = "ManagerRole";

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public const string PerformerRole = "PerformerRole";

        public const string CreatorRoleCaption = LocalizationAlias.Roles.CreatorRole;

        public const string ManagerRoleCaption = LocalizationAlias.Roles.ManagerRole;

        public const string PerformerRoleCaption = LocalizationAlias.Roles.PerformerRole;
        
    }
}