﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using DeliveryTracker.References;
using DeliveryTrackerWeb.ViewModels;
using Xunit;

namespace DeliveryTrackerWeb.Tests.Integration
{
    public class ReferenceTest : FunctionalTestBase
    {
        
        [Fact]
        public async void TestClient()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/create"),
                new ReferenceRequest
                {
                    Entity = new Client
                    {
                        Surname = "Petrov",
                        Name = "Ivan",
                        Patronymic = "Alexeevich",
                        PhoneNumber = "12313",
                        Addresses = new List<Address>
                        {
                            new Address
                            {
                                RawAddress = "First"
                            },
                            new Address
                            {
                                RawAddress = "Second"
                            }
                        }
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var resultClient = new Client();
            resultClient.SetDictionary(postResult.Result.Entity);
            Assert.Equal("Petrov", resultClient.Surname);
            Assert.Equal("Ivan", resultClient.Name);
            Assert.Equal("Alexeevich", resultClient.Patronymic);
            Assert.Equal("12313", resultClient.PhoneNumber);
            Assert.Equal(2, resultClient.Addresses.Count);
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "First");
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "Second");
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/edit"),
                new ReferenceRequest
                {
                    Entity = new Client
                    {
                        Id = resultClient.Id,
                        Surname = "Ivanov",
                        Addresses = new List<Address>
                        {
                            new Address
                            {
                                Id = resultClient.Addresses.First(a => a.RawAddress == "First").Id,
                                Action = CollectionEntityAction.Delete,
                                RawAddress = "First"
                            },
                            new Address
                            {
                                Action = CollectionEntityAction.Edit,
                                Id = resultClient.Addresses.First(a => a.RawAddress == "Second").Id,
                                RawAddress = "Second Edit"
                            },
                            new Address
                            {
                                Action = CollectionEntityAction.Create,
                                RawAddress = "Third"
                            },
                        }
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            resultClient.SetDictionary(editResult.Result.Entity);
            Assert.Equal("Ivanov", resultClient.Surname);
            Assert.Equal(2, resultClient.Addresses.Count);
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "Second Edit");
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "Third");

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Client)}/get?id={resultClient.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            resultClient.SetDictionary(getResult.Result.Entity);
            Assert.Equal("Ivanov", resultClient.Surname);
            Assert.Equal("Ivan", resultClient.Name);
            Assert.Equal("Alexeevich", resultClient.Patronymic);
            Assert.Equal("12313", resultClient.PhoneNumber);
            Assert.Equal(2, resultClient.Addresses.Count);
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "Second Edit");
            Assert.Contains(resultClient.Addresses, address => address.RawAddress == "Third");

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
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/create"),
                new ReferenceRequest
                {
                    Entity = new PaymentType()
                    {
                        Name = "Visa",
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var result = new PaymentType();
            result.SetDictionary(postResult.Result.Entity);
            Assert.Equal("Visa", result.Name);
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/edit"),
                new ReferenceRequest
                {
                    Entity = new PaymentType()
                    {
                        Id = result.Id,
                        Name = "MasterCard",
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            result.SetDictionary(editResult.Result.Entity);
            Assert.Equal("MasterCard", result.Name);

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/get?id={result.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            result.SetDictionary(getResult.Result.Entity);
            Assert.Equal("MasterCard", result.Name);

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/delete"),
                new ReferenceRequest
                {
                    Id = result.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(PaymentType)}/get?id={result.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }   
        
        
        [Fact]
        public async void TestProduct()
        {
            var (client, _, _, _) = await this.CreateNewHttpClientAndInstance();
            
            var postResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/create"),
                new ReferenceRequest
                {
                    Entity = new Product()
                    {
                        Name = "Sushi",
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, postResult.StatusCode);
            var result = new Product();
            result.SetDictionary(postResult.Result.Entity);
            Assert.Equal("Sushi", result.Name);
            
            var editResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/edit"),
                new ReferenceRequest
                {
                    Entity = new Product()
                    {
                        Id = result.Id,
                        Name = "Pizza",
                    }.GetDictionary(),
                });
            
            Assert.Equal(HttpStatusCode.OK, editResult.StatusCode);
            result.SetDictionary(editResult.Result.Entity);
            Assert.Equal("Pizza", result.Name);

            var getResult = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/get?id={result.Id}"));
            Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
            result.SetDictionary(getResult.Result.Entity);
            Assert.Equal("Pizza", result.Name);

            var deleteResult = await RequestPost<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/delete"),
                new ReferenceRequest
                {
                    Id = result.Id
                });
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);
            
            var getResultNotFound = await RequestGet<ReferenceResponse>(
                client,
                ReferenceUrl($"{nameof(Product)}/get?id={result.Id}"));
            Assert.Equal(HttpStatusCode.BadRequest, getResultNotFound.StatusCode);
        }   
    }
}