using System;
using DeliveryTracker.Localization;

namespace DeliveryTracker.Identification
{
    public static class DefaultRoles
    {
        public static readonly Guid CreatorRole =
            new Guid(0xC3_9E_67_B9, 0x5F_70, 0x47_F3, 0x86, 0x1B, 0x25, 0x66, 0xF4, 0x21, 0x23, 0xB9);
             
        public static readonly Guid ManagerRole =
            new Guid(0x5c_79_70_67, 0xeb_d1, 0x4a_62, 0xa6, 0x35, 0xb8, 0x49, 0x37, 0x13, 0xc1, 0x36);
        
        public static readonly Guid PerformerRole =
            new Guid(0x41_0b_ca_25, 0x2_fc, 0x4d_e5, 0xbe, 0x75, 0xc1, 0xdd, 0x42, 0xb6, 0x05, 0x82);
        
        public const string CreatorRoleName = LocalizationAlias.Roles.CreatorRole;

        public const string ManagerRoleName = LocalizationAlias.Roles.ManagerRole;

        public const string PerformerRoleName = LocalizationAlias.Roles.PerformerRole;
        
    }
}