using System;
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
        /// Создатель компании.
        /// </summary>
        public static readonly Guid CreatorRole = 
            new Guid(0xfbe65847, 0x57c0, 0x42a9, 0x84, 0xa9, 0x3f, 0x95, 0xb9, 0x2f, 0xd3, 0x9e);

        /// <summary>
        /// Менеджер.
        /// </summary>
        public static readonly Guid ManagerRole = 
            new Guid(0xca4e3a74, 0x86bb, 0x4c6e, 0x84, 0xb5, 0x9e, 0x2d, 0xa4, 0x7d, 0x1b, 0x2e);

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public static readonly Guid PerformerRole = 
            new Guid(0xaa522dd3, 0x3a11, 0x46a0, 0xaf, 0xa7, 0x26, 0x0b, 0x70, 0x60, 0x95, 0x24);

        
        /// <summary>
        /// Создатель.
        /// </summary>
        public const string CreatorRoleName = "CreatorRole";
        
        /// <summary>
        /// Менеджер.
        /// </summary>
        public const string ManagerRoleName = "ManagerRole";

        /// <summary>
        /// Исполнитель.
        /// </summary>
        public const string PerformerRoleName = "PerformerRole";

        /// <summary>
        /// Оторабажемое название роли создателя.
        /// </summary>
        public const string CreatorRoleCaption = LocalizationAlias.Roles.CreatorRole;

        /// <summary>
        /// Отображаемое название роли менеджера.
        /// </summary>
        public const string ManagerRoleCaption = LocalizationAlias.Roles.ManagerRole;

        /// <summary>
        /// Отображаемое название роли исполнителя.
        /// </summary>
        public const string PerformerRoleCaption = LocalizationAlias.Roles.PerformerRole;
     
        /// <summary>
        /// Все стандартные роли
        /// </summary>
        public static readonly IReadOnlyList<Guid> AllRoles = new List<Guid>
        {
            CreatorRole,
            ManagerRole,
            PerformerRole,
        }.AsReadOnly();
        
    }
}