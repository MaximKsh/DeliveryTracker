using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.References;

namespace DeliveryTracker.Views.References
{
    public class ReferenceViewGroup : ViewGroupBase
    {
        public ReferenceViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor,
            IReferenceService<Product> prs,
            IReferenceService<Client> crs,
            IReferenceService<PaymentType> ptrs,
            IReferenceService<Warehouse> wrs) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();
            
            var productsView = new ProductsView(1, prs);
            dict[productsView.Name] = productsView;
            var paymentTypeView = new PaymentTypesView(2, ptrs);
            dict[paymentTypeView.Name] = paymentTypeView;
            var clientsView = new ClientsView(3, crs);
            dict[clientsView.Name] = clientsView;
            var warehousesView = new WarehousesView(4, wrs);
            dict[warehousesView.Name] = warehousesView;

            this.Views = new ReadOnlyDictionary<string, IView>(dict);
        }

        /// <inheritdoc />
        public override string Name { get; } = nameof(ReferenceViewGroup);
    }
}