using System.Collections.Generic;
using System.Collections.Immutable;
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
            
            var managersView = new ManagersView();
            dict[managersView.Name] = managersView;
            var performersView = new PerformersView();
            dict[performersView.Name] = performersView;
            var invitationsView = new InvitationsView();
            dict[invitationsView.Name] = invitationsView;

            this.Views = dict.ToImmutableDictionary();
        }
    }
}