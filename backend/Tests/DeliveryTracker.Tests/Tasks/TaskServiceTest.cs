using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Tasks
{
    public class TaskServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IReferenceFacade referenceFacade;

        private readonly ITaskService taskService;
        
        private readonly Instance defaultInstance;

        private readonly User me;

        public TaskServiceTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create().Connect())
            {
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                this.me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(this.me));
            }
            
            var services = new ServiceCollection();
            this.serviceProvider = services
                .AddDeliveryTrackerIdentification(this.Configuration)
                .AddDeliveryTrackerReferences()
                .AddDeliveryTrackerTasks()
                
                .AddSingleton(accessor.Object)
                .AddSingleton(this.Cp)
                .AddSingleton(this.SettingsStorage)
                
                .BuildServiceProvider();

            this.referenceFacade = this.serviceProvider.GetService<IReferenceFacade>();
            this.taskService = this.serviceProvider.GetService<ITaskService>();
        }

        [Fact]
        public async void CreateTaskSimple()
        {
            // Arrange
            var task = new TaskInfo
            {
                TaskNumber = "001"
            };
            
            // Act
            var result = await this.taskService.CreateAsync(task);
            
            // Assert
            Assert.True(result.Success, result.Errors.ErrorsToString());
            var createdTask = result.Result;
            Assert.Equal(this.defaultInstance.Id, createdTask.InstanceId);
            Assert.Equal(this.me.Id, createdTask.AuthorId);
            Assert.Equal(task.TaskNumber, task.TaskNumber);
        }

        [Fact]
        public async void CreateTaskFull()
        {
            // Arrange
            var product1 = new Product {Name = "Product1"};
            var product2 = new Product {Name = "Product2"};
            var warehouse = new Warehouse {Name = "Warehouse"};
            var paymentType = new Warehouse {Name = "PaymentType"};
            var client = new Client
            {
                Name = "client1",
                Addresses = new List<Address>
                {
                    new Address {RawAddress = "Address1", Action = CollectionEntityAction.Create}
                }
            };

            var productId1 = (await this.referenceFacade.CreateAsync(nameof(Product), product1.GetDictionary())).Result.Id;
            var productId2 = (await this.referenceFacade.CreateAsync(nameof(Product), product2.GetDictionary())).Result.Id;
            var warehouseId = (await this.referenceFacade.CreateAsync(nameof(Warehouse), warehouse.GetDictionary())).Result.Id;
            var paymentTypeId = (await this.referenceFacade.CreateAsync(nameof(PaymentType), paymentType.GetDictionary())).Result.Id;
            var clientResult = (await this.referenceFacade.CreateAsync(nameof(Client), client.GetDictionary())).Result.Cast<Client>();
            var clientId = clientResult.Id;
            var clientAddressId = clientResult.Addresses.First().Id;

            var task = new TaskInfo
            {
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 100,
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct {ProductId = productId1, Quantity = 2},
                    new TaskProduct {ProductId = productId2, Quantity = 1}
                },
                WarehouseId = warehouseId,
                PaymentTypeId = paymentTypeId,
                ClientId = clientId,
                ClientAddressId = clientAddressId,
            };
            
            // Act 
            var createResult = await this.taskService.CreateAsync(task);
            var createdTask = createResult.Result;
            var fillResult = await this.taskService.PackTaskAsync(createdTask);
            var taskPackage = fillResult.Result;
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.True(fillResult.Success, fillResult.Errors.ErrorsToString());
            Assert.Equal(task.TaskNumber, taskPackage.TaskInfo.First().TaskNumber);
            Assert.Equal(task.Comment, taskPackage.TaskInfo.First().Comment);
            Assert.Equal(task.Cost, taskPackage.TaskInfo.First().Cost);
            Assert.Equal(product1.Name, taskPackage.LinkedReferences[productId1.ToString()].Cast<Product>().Name);
            Assert.Equal(product2.Name, taskPackage.LinkedReferences[productId2.ToString()].Cast<Product>().Name);
            Assert.Equal(warehouse.Name, taskPackage.LinkedReferences[warehouseId.ToString()].Cast<Warehouse>().Name);
        }

        [Fact]
        public async void EditTask()
        {
            // Arrange
            var task = new TaskInfo
            {
                TaskNumber = "001"
            };
            var result = await this.taskService.CreateAsync(task);
            var createdTask = result.Result;
            var newInfo = new TaskInfo
            {
                Id = createdTask.Id,
                InstanceId = createdTask.InstanceId,
                TaskNumber = "002",
            };

            // Act
            var editResult = await this.taskService.EditTaskAsync(newInfo);
            
            // Assert
            Assert.True(editResult.Success, editResult.Errors.ErrorsToString());
            var modifiedTask = result.Result;
            Assert.Equal(modifiedTask.TaskNumber, task.TaskNumber);

        }

        [Fact]
        public async void GetAndPackTask()
        {
            // Arrange
            var product1 = new Product {Name = "Product1"};
            var product2 = new Product {Name = "Product2"};
            var warehouse = new Warehouse {Name = "Warehouse"};
            var paymentType = new Warehouse {Name = "PaymentType"};
            var client = new Client
            {
                Name = "client1",
                Addresses = new List<Address>
                {
                    new Address {RawAddress = "Address1", Action = CollectionEntityAction.Create}
                }
            };

            var productId1 = (await this.referenceFacade.CreateAsync(nameof(Product), product1.GetDictionary())).Result.Id;
            var productId2 = (await this.referenceFacade.CreateAsync(nameof(Product), product2.GetDictionary())).Result.Id;
            var warehouseId = (await this.referenceFacade.CreateAsync(nameof(Warehouse), warehouse.GetDictionary())).Result.Id;
            var paymentTypeId = (await this.referenceFacade.CreateAsync(nameof(PaymentType), paymentType.GetDictionary())).Result.Id;
            var clientResult = (await this.referenceFacade.CreateAsync(nameof(Client), client.GetDictionary())).Result.Cast<Client>();
            var clientId = clientResult.Id;
            var clientAddressId = clientResult.Addresses.First().Id;

            var newTask = new TaskInfo
            {
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 100,
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct {ProductId = productId1, Quantity = 2},
                    new TaskProduct {ProductId = productId2, Quantity = 1}
                },
                WarehouseId = warehouseId,
                PaymentTypeId = paymentTypeId,
                ClientId = clientId,
                ClientAddressId = clientAddressId,
            };
            var createResult = await this.taskService.CreateAsync(newTask);
            var createdTask = createResult.Result;
            
            // Act 
            var getTaskResult = await this.taskService.GetTaskAsync(createdTask.Id);
            var task = getTaskResult.Result;
            var fillResult = await this.taskService.PackTaskAsync(task);
            var taskPackage = fillResult.Result;
            
            // Assert
            Assert.True(getTaskResult.Success, getTaskResult.Errors.ErrorsToString());
            Assert.True(fillResult.Success, fillResult.Errors.ErrorsToString());
            Assert.Equal(newTask.TaskNumber, taskPackage.TaskInfo.First().TaskNumber);
            Assert.Equal(newTask.Comment, taskPackage.TaskInfo.First().Comment);
            Assert.Equal(newTask.Cost, taskPackage.TaskInfo.First().Cost);
            Assert.Equal(product1.Name, taskPackage.LinkedReferences[productId1.ToString()].Cast<Product>().Name);
            Assert.Equal(product2.Name, taskPackage.LinkedReferences[productId2.ToString()].Cast<Product>().Name);
            Assert.Equal(warehouse.Name, taskPackage.LinkedReferences[warehouseId.ToString()].Cast<Warehouse>().Name);
        }
        
        [Fact]
        public async void PackTasks()
        {
            // Arrange
            var product1 = new Product {Name = "Product1"};
            var product2 = new Product {Name = "Product2"};
            var product3 = new Product {Name = "Product3"};
            var warehouse1 = new Warehouse {Name = "Warehouse1"};
            var warehouse2 = new Warehouse {Name = "Warehouse2"};
            var paymentType = new Warehouse {Name = "PaymentType"};
            var client = new Client
            {
                Name = "client1",
                Addresses = new List<Address>
                {
                    new Address {RawAddress = "Address1", Action = CollectionEntityAction.Create}
                }
            };

            var productId1 = (await this.referenceFacade.CreateAsync(nameof(Product), product1.GetDictionary())).Result.Id;
            var productId2 = (await this.referenceFacade.CreateAsync(nameof(Product), product2.GetDictionary())).Result.Id;
            var productId3 = (await this.referenceFacade.CreateAsync(nameof(Product), product3.GetDictionary())).Result.Id;
            var warehouseId1 = (await this.referenceFacade.CreateAsync(nameof(Warehouse), warehouse1.GetDictionary())).Result.Id;
            var warehouseId2 = (await this.referenceFacade.CreateAsync(nameof(Warehouse), warehouse2.GetDictionary())).Result.Id;
            var paymentTypeId = (await this.referenceFacade.CreateAsync(nameof(PaymentType), paymentType.GetDictionary())).Result.Id;
            var clientResult = (await this.referenceFacade.CreateAsync(nameof(Client), client.GetDictionary())).Result.Cast<Client>();
            var clientId = clientResult.Id;
            var clientAddressId = clientResult.Addresses.First().Id;

            var newTask1 = new TaskInfo
            {
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 100,
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct {ProductId = productId1, Quantity = 2},
                    new TaskProduct {ProductId = productId2, Quantity = 1}
                },
                WarehouseId = warehouseId1,
                PaymentTypeId = paymentTypeId,
                ClientId = clientId,
                ClientAddressId = clientAddressId,
            };
            var newTask2 = new TaskInfo
            {
                TaskNumber = "002",
                Comment = "Comment",
                Cost = 200,
                TaskProducts = new List<TaskProduct>
                {
                    new TaskProduct {ProductId = productId1, Quantity = 2},
                    new TaskProduct {ProductId = productId3, Quantity = 1}
                },
                WarehouseId = warehouseId2,
                PaymentTypeId = paymentTypeId,
                ClientId = clientId,
                ClientAddressId = clientAddressId,
            };
            var createResult = await this.taskService.CreateAsync(newTask1);
            var createdTask1 = createResult.Result;
            createResult = await this.taskService.CreateAsync(newTask2);
            var createdTask2 = createResult.Result;
            
            // Act 
            var getTaskResult1 = await this.taskService.GetTaskAsync(createdTask1.Id);
            var task1 = getTaskResult1.Result;
            var getTaskResult2 = await this.taskService.GetTaskAsync(createdTask2.Id);
            var task2 = getTaskResult2.Result;
            var fillResult = await this.taskService.PackTasksAsync(new List<TaskInfo> {task1, task2});
            var taskPackage = fillResult.Result;
            
            // Assert
            Assert.True(getTaskResult1.Success, getTaskResult1.Errors.ErrorsToString());
            Assert.True(getTaskResult2.Success, getTaskResult2.Errors.ErrorsToString());
            Assert.True(fillResult.Success, fillResult.Errors.ErrorsToString());
            Assert.Equal(2, taskPackage.TaskInfo.Count);
            Assert.Contains(task1.Id, taskPackage.TaskInfo.Select(p => p.Id));
            Assert.Contains(task2.Id, taskPackage.TaskInfo.Select(p => p.Id));
            Assert.Equal(7, taskPackage.LinkedReferences.Count);
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == productId1.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == productId2.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == productId3.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == warehouseId1.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == warehouseId2.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == paymentTypeId.ToString());
            Assert.Contains(taskPackage.LinkedReferences, p => p.Key == clientId.ToString());
            Assert.Equal(1, taskPackage.LinkedUsers.Count);
            Assert.Contains(taskPackage.LinkedUsers, p => p.Key == this.me.Id.ToString());
            
        }
/*
        [Fact]
        public async void TransitTask()
        {
            // Arrange
            var task = new TaskInfo
            {
                TaskNumber = "001"
            };
            var result = await this.taskService.CreateAsync(task);
            var createdTask = result.Result;
            var transitionId = (await this.taskService.PackTaskAsync(createdTask)).Result.LinkedTaskStateTransitions.First().Id;
            
            // Act
            var transitResult = await this.taskService.TransitAsync(createdTask.Id, transitionId);
            
            // Assert
            Assert.True(transitResult.Success, transitResult.Errors.ErrorsToString());
            Assert.Equal(DefaultTaskStates.InProgress.Id, transitResult.Result.TaskStateId);
        }*/
        
    }
}