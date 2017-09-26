using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Roles;
using DeliveryTracker.ViewModels;
using Newtonsoft.Json;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class ManagerController: BaseControllerTest
    {
        
        [Fact]
        public async Task GetAvailableUsers()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = 
                await CreateGroup(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            var performersUsernames = await MassCreateUsers(client, token, RoleInfo.Performer, "123qQ!", 100);
            var managers = await MassCreateUsers(client, token, RoleInfo.Manager, "123qQ!", 2);
            
            for (var i = 0; i < performersUsernames.Count; i += 2)
            {
                var (_, perfToken, _) = await GetToken(client, performersUsernames[i], "123qQ!", RoleInfo.Performer);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                var response = await client.GetAsync("/performers/active");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            foreach (var manager in managers)
            {
                var (_, managerToken, _) = await GetToken(client, manager, "123qQ!", RoleInfo.Manager);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
                var response = await client.GetAsync("/manager/available_performers");
                var performersList =  
                    JsonConvert.DeserializeObject<List<AvailablePerformerViewModel>>(await response.Content.ReadAsStringAsync());
                
                var equals = performersUsernames
                    .Select((p, i) => new {p, i})
                    .Where(p => p.i % 2 == 1)
                    .OrderBy(p => p.p)
                    .Select(p => p.p)
                    .SequenceEqual(performersList.OrderBy(p => p.UserName).Select(p => p.UserName));
                Assert.True(equals);
            }
            
            for (var i = 0; i < performersUsernames.Count; i += 2)
            {
                var (_, perfToken, _) = await GetToken(client, performersUsernames[i], "123qQ!", RoleInfo.Performer);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                var response = await client.GetAsync("/performers/inactive");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            
            foreach (var manager in managers)
            {
                var (_, managerToken, _) = await GetToken(client, manager, "123qQ!", RoleInfo.Manager);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
                var response = await client.GetAsync("/manager/available_performers");
                var performersList =  
                    JsonConvert.DeserializeObject<List<AvailablePerformerViewModel>>(await response.Content.ReadAsStringAsync());
                Assert.Equal(0, performersList.Count);
            }
        }

        [Fact]
        public async Task AddTask()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = 
                await CreateGroup(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            var managers = await MassCreateUsers(client, token, RoleInfo.Manager, "123qQ!", 1);

            var task1 = new AddTaskViewModel
            {
                Caption = "First task",
                Content = "First task content",
                DeadlineDate = null,
                PerformerUserName = null,
            };
            
            var task1Serialized = JsonConvert.SerializeObject(task1);
            var (_, managerToken, _) = await GetToken(client, managers.First(), "123qQ!", RoleInfo.Performer);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
            var task1Response = await client.PostAsync(
                "/manager/add_task", 
                new StringContent(task1Serialized, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.OK, task1Response.StatusCode);
            
            var response = await client.GetAsync("/manager/my_tasks");
            var performersList =  
                JsonConvert.DeserializeObject<List<TaskPreviewViewModel>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(1, performersList.Count);
            Assert.Equal("First task", performersList.First().Caption);
            
        }
        
        [Fact]
        public async Task Add10Tasks()
        {
            var client = this.Server.CreateClient();
            
            var (userName, displayableName, roleCreateGroup, groupName) = 
                await CreateGroup(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            var managers = await MassCreateUsers(client, token, RoleInfo.Manager, "123qQ!", 1);

            var task1 = new AddTaskViewModel
            {
                Caption = "First task",
                Content = "First task content",
                DeadlineDate = null,
                PerformerUserName = null,
            };
            
            
            var (_, managerToken, _) = await GetToken(client, managers.First(), "123qQ!", RoleInfo.Performer);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);

            Enumerable
                .Range(0, 10)
                .Select(async p =>
                {
                    var task = new AddTaskViewModel
                    {
                        Caption = Guid.NewGuid().ToString(),
                        Content = "Content",
                        DeadlineDate = null,
                        PerformerUserName = null,
                    };
                    
                    var taskSerialized = JsonConvert.SerializeObject(task);
                    var taskResponse = await client.PostAsync(
                        "/manager/add_task",
                        new StringContent(taskSerialized, Encoding.UTF8, "application/json"));
                    Assert.Equal(HttpStatusCode.OK, taskResponse.StatusCode);
                })
                .ToList()
                .ForEach(async p => await p);
            
            var response = await client.GetAsync("/manager/my_tasks");
            var performersList =  
                JsonConvert.DeserializeObject<List<TaskPreviewViewModel>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList.Count);
        }

    }
}