namespace DeliveryTracker.Identification
{
    public static class IdentificationHelper
    {
        public const string UserColumnList = @"
id, 
code, 
surname, 
name, 
patronymic, 
phone_number, 
instance_id
";
        
        public const string UserColumnListWithTableAlias = @"
u.id, 
u.code, 
u.surname, 
u.name, 
u.patronymic, 
u.phone_number, 
u.instance_id
";
        
        public const string RoleColumnList = @"
id, 
name
";
        public const string RoleColumnListWithTableAlias = @"
r.id, 
r.name
";
    }
}