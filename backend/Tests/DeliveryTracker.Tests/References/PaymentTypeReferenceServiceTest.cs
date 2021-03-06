﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.References
{
    public class PaymentTypeReferenceServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IReferenceService<PaymentType> paymentTypeService;

        public PaymentTypeReferenceServiceTest()
        {
            var accessor = new Mock<IUserCredentialsAccessor>();
            using (var conn = this.Cp.Create())
            {
                conn.Connect();
                var defaultInstance = TestHelper.CreateRandomInstance(conn);
                var me = TestHelper.CreateRandomUser(DefaultRoles.ManagerRole, defaultInstance.Id, conn);
                accessor
                    .Setup(x => x.GetUserCredentials())
                    .Returns(new UserCredentials(me));
            }
            this.paymentTypeService = new PaymentTypeReferenceService(this.Cp, accessor.Object);
        }

        [Fact]
        public async void Create()
        {
            // Arrange
            var paymentType = new PaymentType
            {
                Name = "Visa",
            };
            
            // Act
            var createResult = await this.paymentTypeService.CreateAsync(paymentType);
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(paymentType.Name, createResult.Result.Name);
        }
        
        
        [Fact]
        public async void Get()
        {
            // Arrange
            var paymentType = new PaymentType
            {
                Name = "Visa",
            };
            var createResult = await this.paymentTypeService.CreateAsync(paymentType);
            
            // Act
            var getResult = await this.paymentTypeService.GetAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
            Assert.Equal(paymentType.Name, getResult.Result.Name);
        }
        
        [Fact]
        public async void GetList()
        {
            // Arrange    
            var ids = new List<Guid>();
            for (var i = 0; i < 10; i++)
            {
                var entity = new PaymentType
                {
                    Name = i.ToString()
                };
                var createResult = await this.paymentTypeService.CreateAsync(entity);  
                ids.Add(createResult.Result.Id);
            }
            
            // Act
            var getResult = await this.paymentTypeService.GetAsync(ids);
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
            Assert.Equal(ids.OrderBy(p => p), getResult.Result.Select(p => p.Id).OrderBy(p => p));
        }
        
        [Fact]
        public async void Edit()
        {
            // Arrange
            var paymentType = new PaymentType
            {
                Name = "Visa",
            };
            var createResult = await this.paymentTypeService.CreateAsync(paymentType);
            var paymentTypeMasterCard = new PaymentType
            {
                Id = createResult.Result.Id,
                Name = "MasterCard",
            };
            
            // Act
            var editResult = await this.paymentTypeService.EditAsync(paymentTypeMasterCard);
            
            // Assert
            Assert.True(editResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(paymentTypeMasterCard.Name, editResult.Result.Name);
        }
        
        
        [Fact]
        public async void Delete()
        {
            // Arrange
            var paymentType = new PaymentType
            {
                Name = "Pizza",
            };
            var createResult = await this.paymentTypeService.CreateAsync(paymentType);
            
            // Act
            var deleteResult = await this.paymentTypeService.DeleteAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(deleteResult.Success, deleteResult.Errors.ErrorsToString());
        }
    }
}