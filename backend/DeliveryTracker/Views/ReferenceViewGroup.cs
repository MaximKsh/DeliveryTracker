﻿using System.Collections.Generic;
using System.Collections.Immutable;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;

namespace DeliveryTracker.Views
{
    public class ReferenceViewGroup : ViewGroupBase
    {
        public ReferenceViewGroup(
            IPostgresConnectionProvider cp,
            IUserCredentialsAccessor accessor) : base(cp, accessor)
        {
            var dict = new Dictionary<string, IView>();
            
            var productsView = new ProductsView(1);
            dict[productsView.Name] = productsView;
            var paymentTypeView = new PaymentTypesView(2);
            dict[paymentTypeView.Name] = paymentTypeView;
            var clientsView = new ClientsView(3);
            dict[clientsView.Name] = clientsView;
            var warehousesView = new WarehousesView();
            dict[warehousesView.Name] = warehousesView;

            this.Views = dict.ToImmutableDictionary();
        }

        /// <inheritdoc />
        public override string Name { get; } = nameof(ReferenceViewGroup);
    }
}