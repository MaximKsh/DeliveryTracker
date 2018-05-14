using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;
using DeliveryTracker.Views.Statistics;
using DeliveryTracker.Views.Tasks;
using DeliveryTracker.Views.Users;
using DeliveryTrackerWeb.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class IntegrationTest : FunctionalTestBase
    {
        /// <summary>
        /// провека доступности вебсервиса
        /// </summary>
        [Fact]
        public async void WebServiceAlive()
        {
            var client = this.Server.CreateClient();
            var result = await RequestGet(client, ServiceUrl(""));
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        /// <summary>
        /// ТЗ1 проверка создания компании
        /// </summary>
        [Fact]
        public async void CreateInstance()
        {
            var (_, instance, _, _) = await this.CreateNewHttpClientAndInstance();
            Assert.NotNull(instance);
        }
        
        /// <summary>
        /// ТЗ1 загрузка информации о компании
        /// </summary>
        [Fact]
        public async void CreateThenGetInstance()
        {
            var (client, instance, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var getResult = await RequestGet<InstanceResponse>(
                client,
                InstanceUrl("get"));
            
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            Assert.Equal(instance.Id, getResult.Result.Instance.Id);
        }

        /// <summary>
        /// ТЗ 2 3 4 отправка приглашения
        /// </summary>
        [Fact]
        public async void TestInvitations()
        {
            // Создание новой компании для тестов
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            
            // Приглашаем менеджера в компанию
            var inviteManagerResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Role = DefaultRoles.ManagerRole, }
                });
            
            // Приглашаем исполнителя в компанию
            var invitePerformerResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User { Surname = "Ivanov", Role = DefaultRoles.PerformerRole, }
                });
            var mInvCode = inviteManagerResult.Result.Invitation.InvitationCode;
            var pInvCode = invitePerformerResult.Result.Invitation.InvitationCode;

            // Проверяем успешность приглашения
            Assert.Equal(HttpStatusCode.OK, inviteManagerResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, invitePerformerResult.StatusCode);
            
            // Загружаем приглашение менеджера
            var getInviteManagerResult = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={inviteManagerResult.Result.Invitation.InvitationCode}"));
            // Загружаем приглашение исполнителя
            var getInvitePerformerResult = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={invitePerformerResult.Result.Invitation.InvitationCode}"));
            
            // Приглашения созданы и совпадают с созданными
            Assert.Equal(HttpStatusCode.OK, getInviteManagerResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getInvitePerformerResult.StatusCode);
            Assert.Equal(mInvCode, getInviteManagerResult.Result.Invitation.InvitationCode);
            Assert.Equal(pInvCode, getInvitePerformerResult.Result.Invitation.InvitationCode);

            // Получаем список приглашений
            var invitationListResponse = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(UserViewGroup)}/{nameof(InvitationsView)}"));
            
            // Проверяем что в нем действительно два созданных ранее приглашения
            Assert.Equal(HttpStatusCode.OK, getInviteManagerResult.StatusCode);
            var viewResult = invitationListResponse.Result.ViewResult.ToList();
            Assert.Equal(2, viewResult.Count);
            var performerInvitationFromView = new Invitation();
            performerInvitationFromView.SetDictionary(viewResult[0]);
            Assert.Equal(pInvCode, performerInvitationFromView.InvitationCode);
            var managerInvitationFromView = new Invitation();
            managerInvitationFromView.SetDictionary(viewResult[1]);
            Assert.Equal(mInvCode, managerInvitationFromView.InvitationCode);
            
            var deleteManagerInvitationResult = await RequestPost(
                client,
                InvitationUrl("delete"),
                new InvitationRequest { Code = inviteManagerResult.Result.Invitation.InvitationCode});
            var deletePerformerInvitationResult = await RequestPost(
                client,
                InvitationUrl("delete"),
                new InvitationRequest {Code = invitePerformerResult.Result.Invitation.InvitationCode});
            Assert.Equal(HttpStatusCode.OK, deleteManagerInvitationResult.StatusCode);
            Assert.Equal(HttpStatusCode.OK, deletePerformerInvitationResult.StatusCode);
            
            var getInviteManagerResultAfterDelete = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={inviteManagerResult.Result.Invitation.InvitationCode}"));
            
            var getInvitePerformerResultAfterDelete = await RequestGet<InvitationResponse>(
                client,
                InvitationUrl($"get?code={invitePerformerResult.Result.Invitation.InvitationCode}"));
            
            Assert.Equal(HttpStatusCode.BadRequest, getInviteManagerResultAfterDelete.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, getInvitePerformerResultAfterDelete.StatusCode);
            Assert.All(getInviteManagerResultAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.InvitationNotFound, error.Code));
            Assert.All(getInvitePerformerResultAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.InvitationNotFound, error.Code));
        }

        /// <summary>
        /// Принятие приглашения
        /// </summary>
        [Fact]
        public async void InviteLoginAndGet()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, manager) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            
            var getManagerResult = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            
            Assert.Equal(HttpStatusCode.OK, getManagerResult.StatusCode);
            Assert.Equal(manager.Id, getManagerResult.Result.User.Id);
            
            
            var (performerClient, performer) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            
            var getPerformerResult = await RequestGet<AccountResponse>(
                performerClient,
                AccountUrl("about"));
            
            Assert.Equal(HttpStatusCode.OK, getPerformerResult.StatusCode);
            Assert.Equal(performer.Id, getPerformerResult.Result.User.Id);
        }

        /// <summary>
        /// ТЗ 5
        /// </summary>
        [Fact]
        public async void GetUserList()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            
            var managersListResponse = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(UserViewGroup)}/{nameof(ManagersView)}"));
            var viewResult = managersListResponse.Result.ViewResult.ToList();
            Assert.Equal(3, viewResult.Count);
            
            var performersListResponse = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(UserViewGroup)}/{nameof(PerformersView)}"));
            var viewPerformersResult = performersListResponse.Result.ViewResult.ToList();
            Assert.Equal(1, viewPerformersResult.Count);
        }
        
        /// <summary>
        /// ТЗ п6
        /// </summary>
        [Fact]
        public async void DeleteUser()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (_, usr) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            
            var response = await RequestPost<UserResponse>(
                client,
                UserUrl($"delete"),
                new UserRequest { Id = usr.Id});
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// ТЗ 7 8 9    13
        /// </summary>
        [Fact]
        public async void CreateAndEditTask()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            var (performerClient, perf) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            var products = new[]
                {await CreateProduct(managerClient, "Кола"), await CreateProduct(managerClient, "Пицца"),};
            
            
            var task = await TestCreateTask(managerClient, perf.Id, products);

            var editedTask = await TestEditTask(managerClient, task, products);

            await TestWorkflow(managerClient, performerClient, editedTask);
        }

        /// <summary>
        /// ТЗ 10
        /// </summary>
        [Fact]
        public async void GetTasks()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            var (performerClient, perf) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            var products = new[]
                {await CreateProduct(managerClient, "Кола"), await CreateProduct(managerClient, "Пицца"),};
            
            var package = await TestCreateTask(managerClient, perf.Id, products);
            var response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Queue)});
            package = response.Result.TaskPackage;
            await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Waiting)});
            await TestCreateTask(managerClient, null, products);
            
            var managerViewResult = await RequestGet<ViewResponse>(
                managerClient,
                ViewUrl($"{nameof(TaskViewGroup)}/{nameof(MyTasksManagerView)}"));
            var viewResult = managerViewResult.Result.ViewResult.ToList();
            Assert.Equal(2, (viewResult[0][nameof(TaskPackage.TaskInfo)] as JArray)?.Count);
            
            var performerViewResponse = await RequestGet<ViewResponse>(
                performerClient,
                ViewUrl($"{nameof(TaskViewGroup)}/{nameof(ActualTasksPerformerView)}"));
            var performerViewResult = performerViewResponse.Result.ViewResult.ToList();
            Assert.Equal(1, (performerViewResult[0][nameof(TaskPackage.TaskInfo)] as JArray)?.Count);
        }
        
        /// <summary>
        /// ТЗ 11
        /// </summary>
        [Fact]
        public async void EditProfile()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var result = await RequestPost<AccountResponse>(
                client,
                AccountUrl("edit"),
                new AccountRequest
                {
                    User = new User
                    {
                        Surname = "Sidorov",
                        Name = "Petr",
                        PhoneNumber = "+7(909)765-43-12"
                    }
                });
            
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
        
        /// <summary>
        /// ТЗ 11
        /// </summary>
        [Fact]
        public async void ChangePassword()
        {
            var (creatorClient, _, creator, _) = await this.CreateNewHttpClientAndInstance();

            var changePasswordResult = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("change_password"),
                new AccountRequest
                {
                    CodePassword = new CodePassword {Password = CorrectPassword},
                    NewCodePassword = new CodePassword {Password = CorrectPassword + CorrectPassword}
                });
            
            Assert.Equal(HttpStatusCode.OK, changePasswordResult.StatusCode);
            var loginResultOldPassword = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = creator.Code,
                        Password = CorrectPassword,
                    }
                });
            Assert.Equal(HttpStatusCode.Forbidden, loginResultOldPassword.StatusCode);
            
            var loginResult = await RequestPost<AccountResponse>(
                creatorClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = creator.Code,
                        Password = CorrectPassword + CorrectPassword,
                    }
                });
            Assert.Equal(HttpStatusCode.OK, loginResult.StatusCode);
        }

        /// <summary>
        /// ТЗ 12
        /// </summary>
        [Fact]
        public async void Statistics()
        {
            var (client, _, manager, _) = await this.CreateNewHttpClientAndInstance();
            var (performerClient, perf) = await this.CreateUserViaInvitation(client, DefaultRoles.PerformerRole);
            var products = new[]
                {await CreateProduct(client, "Кола"), await CreateProduct(client, "Пицца"),};

            var task = await TestCreateTask(client, perf.Id, products);
            var editedTask = await TestEditTask(client, task, products);
            await TestWorkflow(client, performerClient, editedTask);
            task = await TestCreateTask(client, perf.Id, products);
            editedTask = await TestEditTask(client, task, products);
            await TestWorkflow(client, performerClient, editedTask);

            var managerStatistics1 = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(StatisticsViewGroup)}/{nameof(PerformanceStatisticsView)}?author_id={manager.Id}"));
            
            var managerStatistics2 = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(StatisticsViewGroup)}/{nameof(ActualTasksStateDistributionView)}?author_id={manager.Id}"));

            var performerStatistics1 = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(StatisticsViewGroup)}/{nameof(PerformanceStatisticsView)}?performer_id={perf.Id}"));
            
            var performerStatistics2 = await RequestGet<ViewResponse>(
                client,
                ViewUrl($"{nameof(StatisticsViewGroup)}/{nameof(ActualTasksStateDistributionView)}?performer_id={perf.Id}"));
        }

        
        [Fact]
        public async void TestClient()
        {
            var (client, inst, _, _) = await this.CreateNewHttpClientAndInstance();
            var clientId = Guid.NewGuid();
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var thirdId = Guid.NewGuid();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/create"),
                new ReferenceRequest
                {
                    Entity = new ReferencePackage
                    {
                        Entry = new Client
                        {
                            Id = clientId,
                            Surname = "Petrov",
                            Name = "Ivan",
                            Patronymic = "Alexeevich",
                            PhoneNumber = "12313",
                        },
                        Collections = new List<ReferenceCollectionBase>
                        {
                            new ClientAddress
                            {
                                Id = firstId,
                                ParentId = clientId,
                                Action = ReferenceAction.Create,
                                RawAddress = "First"
                            },
                            new ClientAddress
                            {
                                Id = secondId,
                                ParentId = clientId,
                                Action = ReferenceAction.Create,
                                RawAddress = "Second"
                            }
                        }
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var resultPackage = new ReferencePackage();
            resultPackage.SetDictionary(postResult.Result.Entity);
            var resultClient = resultPackage.Entry.Cast<Client>();
            Assert.Equal("Petrov", resultClient.Surname);
            Assert.Equal("Ivan", resultClient.Name);
            Assert.Equal("Alexeevich", resultClient.Patronymic);
            Assert.Equal("12313", resultClient.PhoneNumber);
            Assert.Equal(2, resultPackage.Collections.Count);
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "First");
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "Second");
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/edit"),
                new ReferenceRequest
                {
                    Entity = new ReferencePackage()
                    {
                        Entry =new Client
                        {
                            Id = resultClient.Id,
                            InstanceId = inst.Id,
                            Surname = "Ivanov",
                        },
                        Collections = new List<ReferenceCollectionBase>
                        {
                            new ClientAddress
                            {
                                Id = firstId,
                                ParentId = clientId,
                                InstanceId = inst.Id,
                                Action = ReferenceAction.Delete,
                                RawAddress = "First"
                            },
                            new ClientAddress
                            {
                                Id = secondId,
                                ParentId = clientId,
                                InstanceId = inst.Id,
                                Action = ReferenceAction.Edit,
                                RawAddress = "Second Edit"
                            },
                            new ClientAddress
                            {
                                Id = thirdId,
                                ParentId = clientId,
                                InstanceId = inst.Id,
                                Action = ReferenceAction.Create,
                                RawAddress = "Third"
                            },
                        }
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            resultPackage.SetDictionary(editResult.Result.Entity);
            resultClient = resultPackage.Entry.Cast<Client>();
            Assert.Equal("Ivanov", resultClient.Surname);
            Assert.Equal(2, resultPackage.Collections.Count);
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "Second Edit");
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "Third");

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/get?id={resultClient.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            resultPackage.SetDictionary(getResult.Result.Entity);
            resultClient = resultPackage.Entry.Cast<Client>();
            Assert.Equal("Ivanov", resultClient.Surname);
            Assert.Equal("Ivan", resultClient.Name);
            Assert.Equal("Alexeevich", resultClient.Patronymic);
            Assert.Equal("12313", resultClient.PhoneNumber);
            Assert.Equal(2, resultPackage.Collections.Count);
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "Second Edit");
            Assert.Contains(resultPackage.Collections.Select(p => p.Cast<ClientAddress>()), address => address.RawAddress == "Third");

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/delete"),
                new ReferenceRequest
                {
                    Id = resultClient.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/get?id={resultClient.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }
        
        [Fact]
        public async void TestPaymentType()
        {
            var (client, inst, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/create"),
                new ReferenceRequest
                {
                    Entity = new PaymentType()
                    {
                        Name = "Visa",
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var result = new ReferencePackage();
            result.SetDictionary(postResult.Result.Entity);
            var paymentType = result.Entry.Cast<PaymentType>();
            Assert.Equal("Visa", paymentType.Name);
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/edit"),
                new ReferenceRequest
                {
                    Entity = new PaymentType()
                    {
                        Id = paymentType.Id,
                        InstanceId = inst.Id,
                        Name = "MasterCard",
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            result.SetDictionary(editResult.Result.Entity);
            paymentType = result.Entry.Cast<PaymentType>();
            Assert.Equal("MasterCard", paymentType.Name);

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/get?id={paymentType.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            result.SetDictionary(getResult.Result.Entity);
            Assert.Equal("MasterCard", paymentType.Name);

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/delete"),
                new ReferenceRequest
                {
                    Id = paymentType.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/get?id={paymentType.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }   
        
        
        [Fact]
        public async void TestProduct()
        {
            var (client, inst, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/create"),
                new ReferenceRequest
                {
                    Entity = new Product()
                    {
                        Name = "Sushi",
                        Cost = 100,
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var result = new ReferencePackage();
            result.SetDictionary(postResult.Result.Entity);
            var product = result.Entry.Cast<Product>();
            Assert.Equal("Sushi", product.Name);
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/edit"),
                new ReferenceRequest
                {
                    Entity = new Product()
                    {
                        InstanceId = inst.Id,
                        Id = product.Id,
                        Name = "Pizza",
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            result.SetDictionary(editResult.Result.Entity);
            product = result.Entry.Cast<Product>();
            Assert.Equal("Pizza", product.Name);

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/get?id={product.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            result.SetDictionary(getResult.Result.Entity);
            product = result.Entry.Cast<Product>();
            Assert.Equal("Pizza", product.Name);

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/delete"),
                new ReferenceRequest
                {
                    Id = product.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/get?id={product.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }
        
        [Fact]
        public async void TestWarehouse()
        {
            var (client, inst, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/create"),
                new ReferenceRequest
                {
                    Entity = new Warehouse()
                    {
                        Name = "Dom1",
                        RawAddress = "Ulitsa pushkina dom kolotushkina"
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var result = new ReferencePackage();
            result.SetDictionary(postResult.Result.Entity);
            var warehouse = result.Entry.Cast<Warehouse>();
            Assert.Equal("Dom1", warehouse.Name);
            Assert.Equal("Ulitsa pushkina dom kolotushkina", warehouse.RawAddress);
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/edit"),
                new ReferenceRequest
                {
                    Entity = new Warehouse()
                    {
                        Id = warehouse.Id,
                        InstanceId = inst.Id,
                        Name = "Dom2",
                    }.Pack().GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            result.SetDictionary(editResult.Result.Entity);
            warehouse = result.Entry.Cast<Warehouse>();
            Assert.Equal("Dom2", warehouse.Name);

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/get?id={warehouse.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);

            result.SetDictionary(getResult.Result.Entity);
            warehouse = result.Entry.Cast<Warehouse>();
            Assert.Equal("Dom2", warehouse.Name);

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/delete"),
                new ReferenceRequest
                {
                    Id = warehouse.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/get?id={warehouse.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }
        
        #region non tt tests
        
        //[Fact]
        public async void RefreshSession()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var inviteResult = await RequestPost<InvitationResponse>(
                client,
                InvitationUrl("create"),
                new InvitationRequest
                {
                    User = new User
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        Role = DefaultRoles.ManagerRole,
                    }
                });

            var managerClient = this.Server.CreateClient();
            var firstLoginResult = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("login"),
                new AccountRequest
                {
                    CodePassword = new CodePassword()
                    {
                        Code = inviteResult.Result.Invitation.InvitationCode,
                        Password = CorrectPassword,
                    }
                });
            SetToken(managerClient, firstLoginResult.Result.Token);


            var refreshResult1 = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("refresh"),
                new AccountRequest {RefreshToken = firstLoginResult.Result.RefreshToken});
            Assert.Equal(HttpStatusCode.OK, refreshResult1.StatusCode);
            
            SetToken(managerClient, refreshResult1.Result.Token);
            var refreshResult2 = await RequestPost<AccountResponse>(
                managerClient,
                AccountUrl("refresh"),
                new AccountRequest {RefreshToken = firstLoginResult.Result.RefreshToken});
            Assert.Equal(HttpStatusCode.Forbidden, refreshResult2.StatusCode);
            
            SetToken(managerClient, firstLoginResult.Result.Token);
            var getResult1 = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.Forbidden, getResult1.StatusCode);
            
            SetToken(managerClient, refreshResult1.Result.Token);
            var getResult2 = await RequestGet<AccountResponse>(
                managerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.OK, getResult2.StatusCode);
        }
        
        //[Fact]
        public async void ManagerOperatePerformer()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            var (managerClient, _) = await this.CreateUserViaInvitation(client, DefaultRoles.ManagerRole);
            
            var (performerClient, performer) = await this.CreateUserViaInvitation(managerClient, DefaultRoles.PerformerRole);
            
            // Проверим от имени исполнителя что действительно создали
            var aboutPerformer1 = await RequestGet<AccountResponse>(
                performerClient,
                AccountUrl("about"));
            Assert.Equal(HttpStatusCode.OK, aboutPerformer1.StatusCode);
            
            // Получаем 
            var getPerformer = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            Assert.Equal(HttpStatusCode.OK, getPerformer.StatusCode);
            
            // Меняем имя
            var editPerformer = await RequestPost<AccountResponse>(
                managerClient,
                UserUrl("edit"),
                new AccountRequest
                {
                    User = new User { Id = performer.Id, Name = "NewUserName" }
                });
            Assert.Equal(HttpStatusCode.OK, editPerformer.StatusCode);
            
            // Получаем, проверяем новое имя
            var getPerformerWithNewName = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            Assert.Equal("NewUserName", getPerformerWithNewName.Result.User.Name);
            
            // Удаляем
            var deletePerformer = await RequestPost<AccountRequest>(
                managerClient,
                UserUrl("delete"),
                new UserRequest { Id = getPerformer.Result.User.Id});
            Assert.Equal(HttpStatusCode.OK, deletePerformer.StatusCode);
            
            // Не получаем
            var getPerformerAfterDelete = await RequestGet<AccountResponse>(
                managerClient,
                UserUrl($"get?id={performer.Id}"));
            
            Assert.Equal(HttpStatusCode.BadRequest, getPerformerAfterDelete.StatusCode);
            Assert.All(getPerformerAfterDelete.Result.Errors, error => Assert.Equal(ErrorCode.UserNotFound, error.Code));
            
        }

        #endregion

        #region subtests

        private static async Task<TaskPackage> TestCreateTask(HttpClient managerClient, Guid? perfId, Guid[] products)
        {
            
            var (cId, aId) = await CreateClientWithAddress(managerClient);
            var taskPackage = new TaskPackage
            {
                TaskInfo = new List<TaskInfo>
                {
                    new TaskInfo
                    {
                        Id = Guid.NewGuid(),
                        TaskNumber = "Доставка-001",
                        DeliveryFrom = DateTime.Now.AddDays(1),
                        DeliveryTo = DateTime.Now.AddDays(1).AddHours(1),
                        PerformerId = perfId,
                        ClientId = cId,
                        ClientAddressId = aId,
                        WarehouseId = await CreateWarehouse(managerClient),
                        PaymentTypeId = await CreatePaymentType(managerClient),
                    }
                },
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct
                    {
                        ProductId = products[0],
                        Quantity = 2,
                    },
                    new TaskProduct
                    {
                        ProductId = products[1],
                        Quantity = 1,
                    }
                }
            };

            var response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("create"),
                new TaskRequest {TaskPackage = taskPackage});
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response.Result.TaskPackage;
        }

        private static async Task<TaskPackage> TestEditTask(
            HttpClient managerClient,
            TaskPackage taskPackage,
            Guid[] products)
        {
            var newTaskPackage = new TaskPackage
            {
                TaskInfo = new List<TaskInfo>
                {
                    new TaskInfo
                    {
                        Id = taskPackage.TaskInfo[0].Id,
                        InstanceId = taskPackage.TaskInfo[0].InstanceId,
                        TaskNumber = "Доставка-002",
                    }
                },
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct
                    {
                        ProductId = products[0],
                        Quantity = 0,
                    },
                    new TaskProduct
                    {
                        ProductId = products[1],
                        Quantity = 4,
                    }
                }
            };
            
            
            var response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("edit"),
                new TaskRequest {TaskPackage = newTaskPackage});
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return response.Result.TaskPackage;
        }

        private static async Task TestWorkflow(
            HttpClient managerClient, 
            HttpClient performerClient,
            TaskPackage task)
        {
            var package = task;
            var response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Queue)});
            package = response.Result.TaskPackage;
            
            response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Waiting)});
            
            response = await RequestGet<TaskResponse>(
                performerClient,
                TaskUrl($"get?id={GetId(package)}"));
            package = response.Result.TaskPackage;
            
            response = await RequestPost<TaskResponse>(
                performerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.IntoWork)});
            package = response.Result.TaskPackage;

            response = await RequestPost<TaskResponse>(
                performerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Delivered)});
            
            response = await RequestGet<TaskResponse>(
                managerClient,
                TaskUrl($"get?id={GetId(package)}"));
            package = response.Result.TaskPackage;
            
            response = await RequestPost<TaskResponse>(
                managerClient,
                TaskUrl("change_state"),
                new TaskRequest {Id = GetId(package), TransitionId = GetTransition(package, DefaultTaskStates.Complete)});
        }
        
        
        
        #endregion
        
        #region private

        
        private static Guid GetId(TaskPackage package)
        {
            return package.TaskInfo[0].Id;
        }

        private static Guid GetTransition(
            TaskPackage package,
            TaskState finalState)
        {
            return package.TaskInfo[0].TaskStateTransitions.First(p => p.FinalState == finalState.Id).Id;
        }

        
        private static async Task<(Guid, Guid)> CreateClientWithAddress(HttpClient client)
        {
            var clientId = Guid.NewGuid();
            var addrId = Guid.NewGuid();
            var clientPack = new ReferencePackage
            {
                Entry = new Client
                {
                    Id = clientId,
                    Surname = "Петров",
                    Name = "Иван",
                    Patronymic = "Алексеевич",
                    PhoneNumber = "+7(912)345-67-89",
                },
                Collections = new List<ReferenceCollectionBase>
                {
                    new ClientAddress
                    {
                        Id = addrId,
                        ParentId = clientId,
                        RawAddress = "МГУ",
                        Action = ReferenceAction.Create,
                    },
                    new ClientAddress
                    {
                        ParentId = clientId,
                        RawAddress = "Кремль",
                        Action = ReferenceAction.Create,
                    }
                }
            };
                
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/create"),
                new ReferenceRequest
                {
                    Entity = clientPack.GetDictionary(),
                });

            var referencePackage = new ReferencePackage();
            referencePackage.SetDictionary(postResult.Result.Entity);
            return (clientId, addrId);
        }


        private static async Task<Guid> CreateProduct(HttpClient client, string name)
        {
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/create"),
                new ReferenceRequest
                {
                    Entity = new Product()
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Cost = 100,
                    }.Pack().GetDictionary(),
                });
            
            var referencePackage = new ReferencePackage();
            referencePackage.SetDictionary(postResult.Result.Entity);
            return referencePackage.Entry.Id;
        }
        
        private static async Task<Guid> CreatePaymentType(HttpClient client)
        {
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/create"),
                new ReferenceRequest
                {
                    Entity = new PaymentType()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Наличные",
                    }.Pack().GetDictionary(),
                });
            
            var referencePackage = new ReferencePackage();
            referencePackage.SetDictionary(postResult.Result.Entity);
            return referencePackage.Entry.Id;
        }
        
        private static async Task<Guid> CreateWarehouse(HttpClient client)
        {
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Warehouse)}/create"),
                new ReferenceRequest
                {
                    Entity = new Warehouse
                    {
                        Id = Guid.NewGuid(),
                        Name = "Мгту",
                    }.Pack().GetDictionary(),
                });
            
            var referencePackage = new ReferencePackage();
            referencePackage.SetDictionary(postResult.Result.Entity);
            return referencePackage.Entry.Id;
        }

        
        #endregion
    }
}