using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryTracker.Identification;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using Moq;
using Xunit;

namespace DeliveryTracker.Tests.References
{
    public class ClientReferenceServiceTest : DeliveryTrackerConnectionTestBase
    {
        private readonly IReferenceService<Client> clientService;

        public ClientReferenceServiceTest()
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
            this.clientService = new ClientReferenceService(this.Cp, accessor.Object);
        }

        public static IEnumerable<object[]> GetClientParameters()
        {
            yield return new object[]
            {
                "Ivanov",
                "Ivan",
                "Ivanovich",
                new List<Address>
                {
                    new Address
                    {
                        RawAddress = "Abcd"
                    },
                    new Address
                    {
                        RawAddress = "asdfasfasd"
                    }
                }
            };
            yield return new object[]
            {
                null,
                null,
                null,
                null
            };
            yield return new object[]
            {
                string.Empty,
                string.Empty,
                string.Empty,
                new List<Address>()
            };
            yield return new object[]
            {
                "Petrov",
                "Petr",
                null,
                null
            };
        }
        
        [Theory]
        [MemberData(nameof(GetClientParameters))]
        public async void CreateClient(string surname, string name, string patronymic, List<Address> addresses)
        {
            // Arrange
            var client = new Client
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                Addresses = addresses
            };
            
            // Act
            var createResult = await this.clientService.CreateAsync(client);
            
            // Assert
            Assert.True(createResult.Success, createResult.Errors.ErrorsToString());
            Assert.Equal(surname, createResult.Result.Surname);
            Assert.Equal(name, createResult.Result.Name);
            Assert.Equal(patronymic, createResult.Result.Patronymic);
            var addressesExpectedFuncArray = (addresses ?? (new List<Address>()))
                .Select(p => (Action<Address>) (a => Assert.Equal(a.RawAddress, p.RawAddress)))
                .ToArray();
            Assert.Collection(
                createResult.Result.Addresses, 
                addressesExpectedFuncArray);
        }

        [Fact]
        public async void EditClient()
        {
            // Arrange
            var clientToCreate = new Client
            {
                Surname = "Ivanov",
                Name = "Ivan",
                Patronymic = "Ivanovich",
                Addresses = new List<Address>
                {
                    new Address
                    {
                        RawAddress = "Zero"
                    },
                    new Address
                    {
                        RawAddress = "First"
                    },
                    new Address
                    {
                        RawAddress = "Second"
                    },
                    new Address
                    {
                        RawAddress = "Third"
                    },
                }
            };
            var createResult = await this.clientService.CreateAsync(clientToCreate);
            var client = createResult.Result;
            var newClientData = new Client
            {
                Id = client.Id,
                Surname = "Petrov",
                Name = "",
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Action = CollectionEntityAction.Delete,
                        Id = client.Addresses.First(p => p.RawAddress == "First").Id,
                    },
                    new Address
                    {
                        Action = CollectionEntityAction.Edit,
                        Id = client.Addresses.First(p => p.RawAddress == "Second").Id,
                        RawAddress = "Second New"
                    },
                    new Address
                    {
                        Action = CollectionEntityAction.Edit,
                        Id = client.Addresses.First(p => p.RawAddress == "Third").Id,
                        RawAddress = "Third New"
                    },
                    new Address
                    {
                        Action = CollectionEntityAction.Create,
                        RawAddress = "New"
                    },
                }
            };
            
            // Act
            var editResult = await this.clientService.EditAsync(newClientData);
            
            // Arrange
            Assert.True(editResult.Success, editResult.Errors.ErrorsToString());
            Assert.Equal("Petrov", editResult.Result.Surname);
            Assert.Equal("", editResult.Result.Name);
            Assert.Equal("Ivanovich", editResult.Result.Patronymic);
            Assert.Equal(4, editResult.Result.Addresses.Count);
            var addresses = new[]
            {
                "Zero",
                "Second New",
                "Third New",
                "New"
            };
            Assert.Contains(editResult.Result.Addresses, address => address.RawAddress == addresses[0]);
            Assert.Contains(editResult.Result.Addresses, address => address.RawAddress == addresses[1]);
            Assert.Contains(editResult.Result.Addresses, address => address.RawAddress == addresses[2]);
            Assert.Contains(editResult.Result.Addresses, address => address.RawAddress == addresses[3]);
        }

        [Fact]
        public async void EditThenGet()
        {
            // Arrange
            var clientToCreate = new Client
            {
                Surname = "Ivanov",
                Name = "Ivan",
                Patronymic = "Ivanovich",
            };
            var createResult = await this.clientService.CreateAsync(clientToCreate);
            var client = createResult.Result;
            var newClientData = new Client
            {
                Id = client.Id,
                Surname = "Petrov",
                Name = "",
            };
            await this.clientService.EditAsync(newClientData);
            
            // Act
            var getResult = await this.clientService.GetAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
            Assert.Equal("Petrov", getResult.Result.Surname);
            Assert.Equal("", getResult.Result.Name);
            Assert.Equal("Ivanovich", getResult.Result.Patronymic);
        }
        
        [Theory]
        [MemberData(nameof(GetClientParameters))]
        public async void GetClient(string surname, string name, string patronymic, List<Address> addresses)
        {
            // Arrange
            var client = new Client
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                Addresses = addresses
            };
            var createResult = await this.clientService.CreateAsync(client);
            
            // Act
            var getResult = await this.clientService.GetAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
            Assert.Equal(surname, getResult.Result.Surname);
            Assert.Equal(name, getResult.Result.Name);
            Assert.Equal(patronymic, getResult.Result.Patronymic);
            var addressesExpectedFuncArray = (addresses ?? (new List<Address>()))
                .Select(p => (Action<Address>) (a => Assert.Equal(a.RawAddress, p.RawAddress)))
                .ToArray();
            Assert.Collection(
                getResult.Result.Addresses, 
                addressesExpectedFuncArray);
        }

        [Fact]
        public async void GetClientList()
        {
            // Arrange    
            var ids = new List<Guid>();
            foreach (var data in GetClientParameters())
            {
                var client = new Client
                {
                    Surname = data[0] as string,
                    Name = data[1] as string,
                    Patronymic = data[2] as string,
                    Addresses = data[3] as List<Address>
                };
                var createResult = await this.clientService.CreateAsync(client);
                ids.Add(createResult.Result.Id);
            }
            
            // Act
            var getResult = await this.clientService.GetAsync(ids);
            
            // Assert
            Assert.True(getResult.Success, getResult.Errors.ErrorsToString());
            Assert.Equal(ids.OrderBy(p => p), getResult.Result.Select(p => p.Id).OrderBy(p => p));
            
            
            
        }
        
        
        [Theory]
        [MemberData(nameof(GetClientParameters))]
        public async void DeleteClient(string surname, string name, string patronymic, List<Address> addresses)
        {
            // Arrange
            var client = new Client
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                Addresses = addresses
            };
            var createResult = await this.clientService.CreateAsync(client);

            // Act
            var deleteResult = await this.clientService.DeleteAsync(createResult.Result.Id);
            
            // Assert
            Assert.True(deleteResult.Success, deleteResult.Errors.ErrorsToString());
        }
        
        [Fact]
        public async void DeleteClientNotFound()
        {
            // Act
            var deleteResult = await this.clientService.DeleteAsync(Guid.NewGuid());
            
            // Assert
            Assert.False(deleteResult.Success, deleteResult.Errors.ErrorsToString());
        }
        
        
    }
}