using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Database;
using DeliveryTracker.Identification;
using DeliveryTracker.Localization;
using DeliveryTracker.Tasks;

namespace DeliveryTracker.Views
{
    public sealed class MyTasksManagerView : UserTasksView
    {
        public MyTasksManagerView(
            int order,
            ITaskService taskService) : base(order, taskService)
        {
        }


        public override string Name { get; } = nameof(MyTasksManagerView);
        public override IReadOnlyList<Guid> PermittedRoles { get; } = new List<Guid>
        {
            DefaultRoles.CreatorRole,
            DefaultRoles.ManagerRole,
        }.AsReadOnly();

        protected override ViewDigest ViewDigestFactory(
            long count)
        {
            return new ViewDigest
            {
                Caption = LocalizationAlias.Views.MyTasksManagerView,
                Count = count,
                EntityType = nameof(TaskInfo),
                Order = this.Order,
                IconName = "Я не знаю"
            };
        }

        public override async Task<ServiceResult<IList<IDictionaryObject>>> GetViewResultAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var newParams = parameters.ToDictionary(p => p.Key, p => p.Value);
            newParams["author_id"] = new List<string> { userCredentials.Id.ToString() }.AsReadOnly() ;
            return await base.GetViewResultAsync(oc, userCredentials, newParams);
        }

        public override async Task<ServiceResult<long>> GetCountAsync(
            NpgsqlConnectionWrapper oc,
            UserCredentials userCredentials,
            IReadOnlyDictionary<string, IReadOnlyList<string>> parameters)
        {
            var newParams = parameters.ToDictionary(p => p.Key, p => p.Value);
            newParams["author_id"] = new List<string> { userCredentials.Id.ToString() }.AsReadOnly() ;
            return await base.GetCountAsync(oc, userCredentials, newParams);
        }
    }
}