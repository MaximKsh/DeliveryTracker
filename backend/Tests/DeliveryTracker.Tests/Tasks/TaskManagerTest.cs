using System;
using System.Collections.Generic;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.References;
using DeliveryTracker.Tasks;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.Tasks
{
    public class TaskManagerTest : DeliveryTrackerConnectionTestBase
    {
        private readonly ITaskManager manager;

        private readonly IReferenceService<Product> productService;

        private readonly Instance defaultInstance;

        private readonly User me;
        
        public TaskManagerTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            this.manager = new TaskManager(this.Cp);
            using (var conn = this.Cp.Create().Connect())
            {
                this.defaultInstance = TestHelper.CreateRandomInstance(conn);
                this.me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, this.defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(this.me));
            }
            this.productService = new ProductReferenceService(this.Cp, accessor.Object);
        }
        
        [Fact]
        public async void Create()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 1000,
                DeliveryCost = 200,
            };
            
            // Act
            var createResult = await this.manager.CreateAsync(taskInfo);
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(this.defaultInstance.Id, createResult.Result.InstanceId);
            Assert.Equal(this.me.Id, createResult.Result.AuthorId);
            Assert.Equal(taskInfo.TaskNumber, createResult.Result.TaskNumber);
            Assert.Equal(taskInfo.Comment, createResult.Result.Comment);
            Assert.Equal(taskInfo.Cost, createResult.Result.Cost);
            Assert.Equal(taskInfo.DeliveryCost, createResult.Result.DeliveryCost);
        }

        [Fact]
        public async void Get()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
            };
            var taskCreateResult = await this.manager.CreateAsync(taskInfo);
            
            // Act
            var taskGetResult = await this.manager.GetAsync(taskCreateResult.Result.Id, this.defaultInstance.Id);
            
            // Assert
            Assert.True(taskGetResult.Success, taskGetResult.Errors.ErrorsToString());
            Assert.Equal(this.defaultInstance.Id, taskGetResult.Result.InstanceId);
            Assert.Equal(this.me.Id, taskGetResult.Result.AuthorId);
            Assert.Equal(taskInfo.TaskNumber, taskGetResult.Result.TaskNumber);
        }

        [Fact]
        public async void GetNotFound()
        {
            // Act
            var taskGetResult = await this.manager.GetAsync(Guid.NewGuid(), this.defaultInstance.Id);
            
            // Assert
            Assert.False(taskGetResult.Success, taskGetResult.Errors.ErrorsToString());
        }

        [Fact]
        public async void Edit()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 1000,
                DeliveryCost = 200,
            };
            var createResult = await this.manager.CreateAsync(taskInfo);
            taskInfo = createResult.Result;

            var editInfo = new TaskInfo
            {
                Id = taskInfo.Id,
                InstanceId = taskInfo.InstanceId,
                Comment = "Comment New",
                Cost = 1100,
                DeliveryCost = 100,
            };
            
            // Act
            var editResult = await this.manager.EditAsync(editInfo);
            
            // Assert
            Assert.True(editResult.Success, editResult.Errors.ErrorsToString());
            Assert.Equal(editInfo.Comment, editResult.Result.Comment);
            Assert.Equal(editInfo.Cost, editResult.Result.Cost);
            Assert.Equal(editInfo.DeliveryCost, editResult.Result.DeliveryCost);

        }

        [Fact]
        public async void Delete()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
                Comment = "Comment",
                Cost = 1000,
                DeliveryCost = 200,
            };
            var createResult = await this.manager.CreateAsync(taskInfo);
            taskInfo = createResult.Result;
            
            // Act
            var deleteResult = await this.manager.DeleteAsync(taskInfo.Id, taskInfo.InstanceId);
            var taskGetResult = await this.manager.GetAsync(taskInfo.Id, this.defaultInstance.Id);
            
            // Assert
            Assert.True(deleteResult.Success, deleteResult.Errors.ErrorsToString());
            Assert.False(taskGetResult.Success, taskGetResult.Errors.ErrorsToString());
        }
        
        [Fact]
        public async void CreateWithProducts()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
            };
            var taskCreateResult = await this.manager.CreateAsync(taskInfo);
            taskInfo = taskCreateResult.Result;
            
            var product = new Product
            {
                Name = "Pizza",
                Description = "Pepperoni",
                Cost = 10,
                VendorCode = "ABC-3"
            };
            var productCreateResult = await this.productService.CreateAsync(product);
            product = productCreateResult.Result;
            var taskProduct = new TaskProduct
            {
                ProductId = product.Id,
                Quantity = 1,
            };
            
            // Act
            var taskProductResult = await this.manager.EditProductsAsync(
                taskInfo.Id,
                taskInfo.InstanceId,
                new List<TaskProduct>() {taskProduct});
            
            // Assert
            Assert.True(taskProductResult.Success, taskProductResult.Errors.ErrorsToString());
        }


        [Fact]
        public async void CreateWithProductsAndGet()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
            };
            var taskCreateResult = await this.manager.CreateAsync(taskInfo);
            taskInfo = taskCreateResult.Result;
            
            var productCreateResult1 = await this.productService.CreateAsync(new Product());
            var productCreateResult2 = await this.productService.CreateAsync(new Product());
            var taskProduct1 = new TaskProduct
            {
                ProductId = productCreateResult1.Result.Id,
                Quantity = 1,
            };
            var taskProduct2 = new TaskProduct
            {
                ProductId = productCreateResult1.Result.Id,
                Quantity = 2,
            };
            var taskProduct3 = new TaskProduct
            {
                ProductId = productCreateResult2.Result.Id,
                Quantity = 1,
            };
            await this.manager.EditProductsAsync(
                taskInfo.Id,
                taskInfo.InstanceId,
                new List<TaskProduct>() {taskProduct1, taskProduct2, taskProduct3});
             
            // Act
            var taskGetResult = await this.manager.GetAsync(taskCreateResult.Result.Id, this.defaultInstance.Id);
            var taskProductsGetResult =
                await this.manager.FillProductsAsync(new List<TaskInfo>{taskGetResult.Result});
            
            // Assert
            var task = taskGetResult.Result;
            Assert.True(taskProductsGetResult.Success, taskGetResult.Errors.ErrorsToString());
            Assert.Equal(2, task.TaskProducts.Count);
        }
        
        [Fact]
        public async void CreateWithProductsThenDeleteProduct()
        {
            // Arrange
            var taskInfo = new TaskInfo
            {
                Id = Guid.NewGuid(),
                InstanceId = this.defaultInstance.Id,
                TaskStateId = DefaultTaskStates.Unconfirmed.Id,
                AuthorId = this.me.Id,
                TaskNumber = "001",
            };
            var taskCreateResult = await this.manager.CreateAsync(taskInfo);
            taskInfo = taskCreateResult.Result;
            
            var productCreateResult1 = await this.productService.CreateAsync(new Product());
            var productCreateResult2 = await this.productService.CreateAsync(new Product());
            var taskProduct1 = new TaskProduct
            {
                ProductId = productCreateResult1.Result.Id,
                Quantity = 2,
            };
            var taskProduct2 = new TaskProduct
            {
                ProductId = productCreateResult2.Result.Id,
                Quantity = 1,
            };
            await this.manager.EditProductsAsync(
                taskInfo.Id,
                taskInfo.InstanceId,
                new List<TaskProduct>() {taskProduct1, taskProduct2});
             
            
            var taskProduct3 = new TaskProduct
            {
                ProductId = productCreateResult1.Result.Id,
                Quantity = -1,
            };
            var taskProduct4 = new TaskProduct
            {
                ProductId = productCreateResult2.Result.Id,
                Quantity = -1,
            };
            // Act
            
            var editResult = await this.manager.EditProductsAsync(
                taskInfo.Id,
                taskInfo.InstanceId,
                new List<TaskProduct>() {taskProduct3, taskProduct4});
            
            var taskGetResult = await this.manager.GetAsync(taskCreateResult.Result.Id, this.defaultInstance.Id);
            await this.manager.FillProductsAsync(new List<TaskInfo>{taskGetResult.Result});
            
            // Assert
            var task = taskGetResult.Result;
            Assert.True(editResult.Success, taskGetResult.Errors.ErrorsToString());
            Assert.Single(task.TaskProducts);
        }
    }
}