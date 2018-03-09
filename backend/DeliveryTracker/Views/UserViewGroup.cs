using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public class UserViewGroup : ViewGroupBase
    {
        public override string Name { get; } = nameof(UserViewGroup);

        public UserViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();
            
            var managersView = new ManagersView(0);
            dict[managersView.Name] = managersView;
            var performersView = new PerformersView(1);
            dict[performersView.Name] = performersView;
            var invitationsView = new InvitationsView(2);
            dict[invitationsView.Name] = invitationsView;

            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }
    }
}