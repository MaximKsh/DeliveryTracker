using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DeliveryTracker.Roles;
using DeliveryTracker.TaskStates;
using DeliveryTracker.Validation;
using DeliveryTracker.ViewModels;
using Newtonsoft.Json;
using Xunit;

namespace DeliveryTrackerTest.Controllers
{
    public class WorkflowTest: BaseControllerTest
    {
        /// <summary>
        /// Создаются 10 исполнителей.
        /// Смотрим активных - 0
        /// Ставим положения каждому нечетному
        /// Смотрим активных - 5
        /// Ставим положения каждому четному
        /// Смотрим активных - 10
        /// Еще раз всем обновляем положение
        /// Смотрим активных - 10
        /// Снимаем троих активных
        /// Смотрим активных - 7
        /// </summary>
        [Fact]
        public async void TestPerformersGeopositions()
        {
            var client = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(client, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(client, userName, "123qQ!", roleCreateGroup);
            var performersUsernames = await MassCreateUsers(client, token, RoleInfo.Performer, "123qQ!", 10);
            var manager = (await MassCreateUsers(client, token, RoleInfo.Manager, "123qQ!", 1)).First();
            var (_, managerToken, _) = await GetToken(client, manager, "123qQ!", RoleInfo.Manager);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
            
            // Смотрим активных
            var response1 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList1 =  
                JsonConvert.DeserializeObject<UserInfoViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(0, performersList1.Length);
            
            // Ставим каждому нечетному 
            for (var i = 0; i < performersUsernames.Count; i += 2)
            {
                var perfClient = this.Server.CreateClient();
                var (_, perfToken, _) = await GetToken(client, performersUsernames[i], "123qQ!", RoleInfo.Performer);
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                await UpdateRandomPosition(perfClient);
            }
            
            // Смотрим активных
            var response2 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList2 =  
                JsonConvert.DeserializeObject<UserInfoViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(5, performersList2.Length);
            var expectedPerformersList1 = performersUsernames
                .Select((p, i) => new {p, i})
                .Where(p => p.i % 2 == 0)
                .OrderBy(p => p.p)
                .Select(p => p.p)
                .ToList();
            var actualPerformersList1 = performersList2
                .OrderBy(p => p.UserName)
                .Select(p => p.UserName)
                .ToList();
            Assert.True(expectedPerformersList1.SequenceEqual(actualPerformersList1));
            
            // Ставим каждому четному 
            for (var i = 1; i < performersUsernames.Count; i += 2)
            {
                var perfClient = this.Server.CreateClient();
                var (_, perfToken, _) = await GetToken(client, performersUsernames[i], "123qQ!", RoleInfo.Performer);
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                await UpdateRandomPosition(perfClient);
            }
            
            // Смотрим активных
            var response3 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList3 =  
                JsonConvert.DeserializeObject<UserInfoViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList3.Length);
            var expectedPerformersList2 = performersUsernames
                .OrderBy(p => p)
                .ToList();
            var actualPerformersList2 = performersList3
                .OrderBy(p => p.UserName)
                .Select(p => p.UserName)
                .ToList();
            Assert.True(expectedPerformersList2.SequenceEqual(actualPerformersList2));
            
            // Ставим каждому нечетному 
            foreach (var t in performersUsernames)
            {
                var perfClient = this.Server.CreateClient();
                var (_, perfToken, _) = await GetToken(client, t, "123qQ!", RoleInfo.Performer);
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                await UpdateRandomPosition(perfClient);
            }
            
            // Смотрим активных
            var response4 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList4 =  
                JsonConvert.DeserializeObject<UserInfoViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList4.Length);
            var expectedPerformersList3 = performersUsernames
                .OrderBy(p => p)
                .ToList();
            var actualPerformersList3 = performersList4
                .OrderBy(p => p.UserName)
                .Select(p => p.UserName)
                .ToList();
            Assert.True(expectedPerformersList3.SequenceEqual(actualPerformersList3));
            
            // Ставим каждому нечетному 
            for (var i = 0; i < 3; i++)
            {
                var perfClient = this.Server.CreateClient();
                var (_, perfToken, _) = await GetToken(client, performersUsernames[i], "123qQ!", RoleInfo.Performer);
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken);
                await SetInactive(perfClient);
            }
            
            // Смотрим активных
            var response5 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList5 =  
                JsonConvert.DeserializeObject<UserInfoViewModel[]>(await response5.Content.ReadAsStringAsync());
            Assert.Equal(7, performersList5.Length);
            var expectedPerformersList4 = performersUsernames
                .Skip(3)
                .OrderBy(p => p)
                .ToList();
            var actualPerformersList4 = performersList5
                .OrderBy(p => p.UserName)
                .Select(p => p.UserName)
                .ToList();
            Assert.True(expectedPerformersList4.SequenceEqual(actualPerformersList4));
        }

        /// <summary>
        /// Создаем двух менеджеров
        /// Создаем двух исполнителей
        /// Создаем один таск без назначения исполнителя.
        /// Смотрим за каждого исполнителя таск.
        /// Берем в работу одним исполнителем
        /// Смотрим за каждого исполнителя(должен быть виден только одному)
        /// Смотрим за каждого менеджера(должен быть виден только одному)
        /// Завершаем исполнителем задание
        /// Смотрим статус менеджером
        /// </summary>
        [Fact]
        public async void TestOneUndistributedTask()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientManager2 = this.Server.CreateClient();
            
            var clientPerformer1 = this.Server.CreateClient();
            var clientPerformer2 = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName, "123qQ!", roleCreateGroup);
            var performers = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 2);
            var managers = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 2));
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            var (_, managerToken2, _) = await GetToken(clientManager2, managers.Last(), "123qQ!", RoleInfo.Manager);
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            var (_, performerToken2, _) = await GetToken(clientPerformer2, performers.Last(), "123qQ!", RoleInfo.Performer);
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2);
            
            // Поставим исполнителя в работу
            await UpdateRandomPosition(clientPerformer1);
            await UpdateRandomPosition(clientPerformer2);

            // Создаем таск
            var taskId = await AddTask(clientManager1, "TaskCaption", "TaskContent");

            await CheckOneUndistributedTask(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateInfo.NewUndistributedState,
                TaskStateInfo.NewUndistributedState);
            
            // Берем в работу за первого исполнителя
            await TryTakeIntoWork(clientPerformer1, taskId);
            
            await CheckOneTask(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateInfo.InWorkState);
            
            // Смотрим задания за исполнителей по нераспределенным
            var response7 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList7 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response7.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList7.Length);
            var response8 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList8 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response8.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList8.Length);
            
            // Пытаемся завершить за второго
            await TryCompleteTask(clientPerformer2, taskId, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            
            // Завершаем задание за первого 
            await TryCompleteTask(clientPerformer1, taskId, TaskStateInfo.PerformedState);
            
            await CheckOneTask(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateInfo.PerformedState);
            
        }

        /// <summary>
        /// Создаем двух менеджеров
        /// Создаем двух исполнителей.
        /// Создаем таск за первого менеджера первому исполнителю
        /// Создаем таск за второго менеджера второму исполнителю
        /// Пытаемся взять чужой таск в работу
        /// Берем свой таск в работу за каждого
        /// Проверяем за каждого что ему виден корректный таск
        /// За первого исполнителя завершаем
        /// За второго исполнителя отменяем
        /// За менеджеров проверяем статус заданий
        /// </summary>
        [Fact]
        public async void TestTwoTasksWithSpecifiedPerformers()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientManager2 = this.Server.CreateClient();
            
            var clientPerformer1 = this.Server.CreateClient();
            var clientPerformer2 = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName, "123qQ!", roleCreateGroup);
            var performers = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 2);
            var managers = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 2));
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            var (_, managerToken2, _) = await GetToken(clientManager2, managers.Last(), "123qQ!", RoleInfo.Manager);
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            var (_, performerToken2, _) = await GetToken(clientPerformer2, performers.Last(), "123qQ!", RoleInfo.Performer);
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2);
            
            // Поставим исполнителя в работу
            await UpdateRandomPosition(clientPerformer1);
            await UpdateRandomPosition(clientPerformer2);
            
            // Создаем таск
            var firstTaskId = await AddTask(clientManager1, "TaskCaption1", "TaskContent1", performers.First());
            var secondTaskId = await AddTask(clientManager2, "TaskCaption2", "TaskContent2", performers.Last());
            

            await CheckTasksForTwoPerformers(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateInfo.NewState);
            
            // Берем в работу за первого исполнителя второе задание
            await TryTakeIntoWork(clientPerformer1, secondTaskId, HttpStatusCode.Forbidden);
            
            // Берем в работу за первого исполнителя второе задание
            await TryTakeIntoWork(clientPerformer2, firstTaskId, HttpStatusCode.Forbidden);
            
            // Берем в работу за первого исполнителя первое задание
            await TryTakeIntoWork(clientPerformer1, firstTaskId);
            
            // Берем в работу за второго исполнителя второе задание
            await TryTakeIntoWork(clientPerformer2, secondTaskId);
            
            await CheckTasksForTwoPerformers(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateInfo.InWorkState);
            
            // Пытаемся завершать чужое задание
            await TryCompleteTask(clientPerformer1, secondTaskId, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientManager2, firstTaskId, TaskStateInfo.CancelledState, HttpStatusCode.Forbidden);
            
            // Завершаем первое задание за первого 
            await TryCompleteTask(clientPerformer1, firstTaskId, TaskStateInfo.PerformedState);
            
            // Отменяем второе задание за второго
            await TryCompleteTask(clientPerformer2, secondTaskId, TaskStateInfo.CancelledState);
            
            
        }

        /// <summary>
        /// Создаем две группы
        /// В каждой группе создаем по менеджеру
        /// Каджый менеджер создает по нераспределенному заданию
        /// Каждый менеджер создает по распределенному заданию
        /// Каждый менеджер пытается выдать задание на исполнителя другой группы - неудачно
        /// Каждый исполнитель смотрит список заданий
        /// Первый исполнитель пытается взять второе задание - неудачно
        /// Второй исполнитель пытается взять первое задание - неудачно
        /// Загружаем таски и смотрим что они не изменились
        /// Первый берет в работу первое задание
        /// Второй берет в работу второе задание
        /// Пытаемся загрузить чужие таски - неудачно
        /// Первый исполнитель пытается завершить второе задание - неудачно
        /// Второй исполнитель пытается завершить первое задание - неудачно
        /// Загружаем таски и смотрим что они не изменились
        /// Первый завершает первое задание
        /// Второй завершает второе задание
        /// Проверяем что состояния завершены
        /// </summary>
        [Fact]
        public async void TestTwoGroups()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientManager2 = this.Server.CreateClient();
            
            var clientPerformer1 = this.Server.CreateClient();
            var clientPerformer2 = this.Server.CreateClient();
            
            var (userName1, _, roleCreateGroup1, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token1, _) = await GetToken(clientManager1, userName1, "123qQ!", roleCreateGroup1);
            var performers1 = await MassCreateUsers(clientManager1, token1, RoleInfo.Performer, "123qQ!", 1);
            var managers1 = (await MassCreateUsers(clientManager1, token1, RoleInfo.Manager, "123qQ!", 1));
            
            var (userName2, _, roleCreateGroup2, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName2, "123qQ!", roleCreateGroup2);
            var performers2 = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 1);
            var managers2 = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 1));
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers1.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            var (_, managerToken2, _) = await GetToken(clientManager2, managers2.First(), "123qQ!", RoleInfo.Manager);
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers1.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            var (_, performerToken2, _) = await GetToken(clientPerformer2, performers2.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2);
            
            // Поставим исполнителя в работу
            await UpdateRandomPosition(clientPerformer1);
            await UpdateRandomPosition(clientPerformer2);

            // Создаем таск
            var taskId11 = await AddTask(clientManager1, "TaskCaption11", "TaskContent11");
            var taskId12 = await AddTask(clientManager1, "TaskCaption12", "TaskContent12", performers1.First());
            var taskId21 = await AddTask(clientManager2, "TaskCaption21", "TaskContent21");
            var taskId22 = await AddTask(clientManager2, "TaskCaption22", "TaskContent22", performers2.First());
            
            await AddTask(clientManager1, "1", "1", performers2.First(), 
                expectedStatus: HttpStatusCode.Forbidden, errorCode:ErrorCode.PerformerInAnotherGroup);
            await AddTask(clientManager2, "1", "1", performers1.First(),
                expectedStatus: HttpStatusCode.Forbidden, errorCode:ErrorCode.PerformerInAnotherGroup);
                
            // Смотрим задания за менеджеров
            var response1 = await clientManager1.GetAsync(ManagerUrl("my_tasks"));
            var tasksList1 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(2, tasksList1.Length);
            Assert.Equal(TaskStateInfo.NewUndistributedState, tasksList1.First().TaskState);
            Assert.Equal("TaskCaption11", tasksList1.First().Caption);
            Assert.Equal("TaskContent11", tasksList1.First().ContentPreview);
            Assert.Equal(TaskStateInfo.NewState, tasksList1.Last().TaskState);
            Assert.Equal("TaskCaption12", tasksList1.Last().Caption);
            Assert.Equal("TaskContent12", tasksList1.Last().ContentPreview);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(2, tasksList2.Length);
            Assert.Equal(TaskStateInfo.NewUndistributedState, tasksList2.First().TaskState);
            Assert.Equal("TaskCaption21", tasksList2.First().Caption);
            Assert.Equal("TaskContent21", tasksList2.First().ContentPreview);
            Assert.Equal(TaskStateInfo.NewState, tasksList2.Last().TaskState);
            Assert.Equal("TaskCaption22", tasksList2.Last().Caption);
            Assert.Equal("TaskContent22", tasksList2.Last().ContentPreview);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(TaskStateInfo.NewUndistributedState, tasksList3.First().TaskState);
            Assert.Equal("TaskCaption11", tasksList3.First().Caption);
            Assert.Equal("TaskContent11", tasksList3.First().ContentPreview);
            
            var response4 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList4.Length);
            Assert.Equal(TaskStateInfo.NewState, tasksList4.First().TaskState);
            Assert.Equal("TaskCaption12", tasksList4.First().Caption);
            Assert.Equal("TaskContent12", tasksList4.First().ContentPreview);
            
            var response5 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList5 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response5.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList5.Length);
            Assert.Equal(TaskStateInfo.NewUndistributedState, tasksList5.First().TaskState);
            Assert.Equal("TaskCaption21", tasksList5.First().Caption);
            Assert.Equal("TaskContent21", tasksList5.First().ContentPreview);
            
            var response6 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList6 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response6.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList6.Length);
            Assert.Equal(TaskStateInfo.NewState, tasksList6.First().TaskState);
            Assert.Equal("TaskCaption22", tasksList6.First().Caption);
            Assert.Equal("TaskContent22", tasksList6.First().ContentPreview);
            
            
            // Пытаемся взять чужие задания в работу
            await TryTakeIntoWork(clientPerformer1, taskId21, HttpStatusCode.Forbidden);
            await TryTakeIntoWork(clientPerformer1, taskId22, HttpStatusCode.Forbidden);
            
            await TryTakeIntoWork(clientPerformer2, taskId11, HttpStatusCode.Forbidden);
            await TryTakeIntoWork(clientPerformer2, taskId12, HttpStatusCode.Forbidden);

            // Загружаем и смотрим что ничего не поменялось
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateInfo.NewState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateInfo.NewState);
            
            // Берем свои задания в работу
            await TryTakeIntoWork(clientPerformer1, taskId11);
            await TryTakeIntoWork(clientPerformer1, taskId12);
            
            await TryTakeIntoWork(clientPerformer2, taskId21);
            await TryTakeIntoWork(clientPerformer2, taskId22);
            
            // Пробуем загружать чужие таски
            await GetTaskByPerformer(clientPerformer1, taskId21, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer1, taskId22, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer2, taskId11, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer2, taskId12, HttpStatusCode.NotFound);

            // Пробуем завершить чужие таски
            await TryCompleteTask(clientPerformer1, taskId21, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer1, taskId22, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer2, taskId11, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer2, taskId12, TaskStateInfo.PerformedState, HttpStatusCode.Forbidden);
            
            // Проверяем что ничего не поменялось
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateInfo.InWorkState);
            
            // Завершаем свои
            await TryCompleteTask(clientPerformer1, taskId11, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer1, taskId12, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer2, taskId21, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer2, taskId22, TaskStateInfo.PerformedState);
            
            // Проверяем что завершились
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateInfo.PerformedState);
        }


        /// <summary>
        /// Отменяются менеджером задания во всех состояниях.
        /// </summary>
        [Fact]
        public async void CancellingTaskByManager()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName, "123qQ!", roleCreateGroup);
            var performers = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 1);
            var managers = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 1));

            var performer = performers.First();
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            
            var taskIUndistributed = await AddTask(clientManager1, "1", "1");
            var taskIdNew = await AddTask(clientManager1, "1", "1", performer);
            var taskIdInWork = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);

            await TryTakeIntoWork(clientPerformer1, taskIdInWork);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);

            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateInfo.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            
            await CheckTaskStateByManager(clientManager1, taskIUndistributed, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByManager(clientManager1, taskIdNew, TaskStateInfo.NewState);
            await CheckTaskStateByManager(clientManager1, taskIdInWork, TaskStateInfo.InWorkState);
            await CheckTaskStateByManager(clientManager1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByManager(clientManager1, taskIdCancelled, TaskStateInfo.CancelledState);

            await TryCancelByManager(clientManager1, taskIUndistributed);
            await TryCancelByManager(clientManager1, taskIdNew);
            await TryCancelByManager(clientManager1, taskIdInWork);
            await TryCancelByManager(clientManager1, taskIdPerformed, HttpStatusCode.BadRequest);
            await TryCancelByManager(clientManager1, taskIdCancelled, HttpStatusCode.BadRequest);
            
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            
            await CheckTaskStateByManager(clientManager1, taskIUndistributed, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdNew, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdInWork, TaskStateInfo.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByManager(clientManager1, taskIdCancelled, TaskStateInfo.CancelledState);
        }


        /// <summary>
        /// Попытка завершать задания не в работе (новые или завершенные)
        /// </summary>
        [Fact]
        public async void CompleteNotInWorkTasks()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName, "123qQ!", roleCreateGroup);
            var performers = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 1);
            var managers = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 1));

            var performer = performers.First();
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            
            var taskIUndistributed = await AddTask(clientManager1, "1", "1");
            var taskIdNew = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelledByManager = await AddTask(clientManager1, "1", "1", performer);
            
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await TryCancelByManager(clientManager1, taskIdCancelledByManager);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateInfo.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.CancelledByManagerState);
            
            await TryCompleteTask(clientPerformer1, taskIUndistributed, TaskStateInfo.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIUndistributed, TaskStateInfo.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdNew, TaskStateInfo.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdNew, TaskStateInfo.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateInfo.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateInfo.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.CancelledState, HttpStatusCode.BadRequest);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateInfo.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateInfo.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.CancelledByManagerState);
        }

        [Fact]
        public async void InWorkNotNewTasks()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var (userName, _, roleCreateGroup, _) = 
                await CreateGroup(clientManager1, "Иванов И.И.", "123qQ!", "Группа1");
            var (_, token, _) = await GetToken(clientManager1, userName, "123qQ!", roleCreateGroup);
            var performers = await MassCreateUsers(clientManager1, token, RoleInfo.Performer, "123qQ!", 1);
            var managers = (await MassCreateUsers(clientManager1, token, RoleInfo.Manager, "123qQ!", 1));

            var performer = performers.First();
            
            var (_, managerToken1, _) = await GetToken(clientManager1, managers.First(), "123qQ!", RoleInfo.Manager);
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1);
            
            var (_, performerToken1, _) = await GetToken(clientPerformer1, performers.First(), "123qQ!", RoleInfo.Performer);
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1);
            
            var taskIdInWork = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelledByManager = await AddTask(clientManager1, "1", "1", performer);
            
            await TryTakeIntoWork(clientPerformer1, taskIdInWork);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await TryCancelByManager(clientManager1, taskIdCancelledByManager);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.CancelledByManagerState);

            await TryTakeIntoWork(clientPerformer1, taskIdInWork, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelledByManager, HttpStatusCode.BadRequest);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateInfo.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateInfo.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateInfo.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateInfo.CancelledByManagerState);
            
        }
        
        #region common

        private static async Task CheckTaskStateByManager(HttpClient client, Guid taskId, string state)
        {
            var task1 = await GetTaskByManager(client, taskId);
            Assert.Equal(state, task1.TaskState);
        }
        
        private static async Task CheckTaskStateByPerformer(HttpClient client, Guid taskId, string state)
        {
            var task1 = await GetTaskByPerformer(client, taskId);
            Assert.Equal(state, task1.TaskState);
        }
        
        private static async Task UpdateRandomPosition(HttpClient client)
        {
            var random = new Random();
            var updatePos1 = new GeopositionViewModel
            {
                Latitude = random.NextDouble() * 100,
                Longitude = random.NextDouble() * 100
            };
            var perfResponse1 = await client.PostAsync(
                PerformerUrl("update_position"), 
                new StringContent(JsonConvert.SerializeObject(updatePos1), Encoding.UTF8, ContentType));
            Assert.Equal(HttpStatusCode.OK, perfResponse1.StatusCode);
        }
        

        private static async Task SetInactive(HttpClient client)
        {
            var perfResponse = await client.PostAsync(PerformerUrl("inactive"), new StringContent(""));
            Assert.Equal(HttpStatusCode.OK, perfResponse.StatusCode);
        }

        private static async Task<Guid> AddTask(
            HttpClient client,
            string caption, 
            string content,
            string performer = null,
            DateTime? deadline = null,
            HttpStatusCode expectedStatus = HttpStatusCode.Created,
            string errorCode = null)
        {
            var taskViewModel = new AddTaskViewModel
            {
                Caption = caption,
                Content = content,
                PerformerUserName = performer,
                DeadlineDate = null,
            };
            var addTaskResponse = await client.PostAsync(
                ManagerUrl("add_task"), 
                new StringContent(JsonConvert.SerializeObject(taskViewModel), Encoding.UTF8, ContentType));
            Assert.Equal(expectedStatus, addTaskResponse.StatusCode);
            if (expectedStatus == HttpStatusCode.Created)
            {
                var task =
                    JsonConvert.DeserializeObject<TaskInfoViewModel>(await addTaskResponse.Content.ReadAsStringAsync());
                Assert.Equal(caption, task.Caption);
                var expectingState = performer != null
                    ? TaskStateInfo.NewState
                    : TaskStateInfo.NewUndistributedState;
                Assert.Equal(expectingState, task.TaskState);
                return task.Id;
            }
            else
            {
                var task =
                    JsonConvert.DeserializeObject<ErrorListViewModel>(await addTaskResponse.Content.ReadAsStringAsync());
                Assert.Equal(errorCode, task.Errors.First().Code);
                return Guid.Empty;
            }
        }

        private static async Task<TaskDetailsViewModel> GetTaskByManager(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var taskResponse = await client.GetAsync(ManagerUrl($"task/{taskId}"));
            Assert.Equal(expectedCode, taskResponse.StatusCode);

            return expectedCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<TaskDetailsViewModel>(await taskResponse.Content.ReadAsStringAsync()) 
                : null;
        }
        
        
        private static async Task<TaskDetailsViewModel> GetTaskByPerformer(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var taskResponse = await client.GetAsync(PerformerUrl($"task/{taskId}"));
            Assert.Equal(expectedCode, taskResponse.StatusCode);

            return expectedCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<TaskDetailsViewModel>(await taskResponse.Content.ReadAsStringAsync()) 
                : null;
        }
        
        private static async Task TryTakeIntoWork(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var inWorkViewModel1 = new TaskInfoViewModel
            {
                Id = taskId,
            };
            var inWorkResponse1 = await client.PostAsync(
                PerformerUrl("take_task_to_work"), 
                new StringContent(JsonConvert.SerializeObject(inWorkViewModel1), Encoding.UTF8, ContentType));
            Assert.Equal(expectedCode, inWorkResponse1.StatusCode);
        }
        
        private static async Task TryCompleteTask(
            HttpClient client,
            Guid taskId,
            string state,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var completeTaskViewModel3 = new TaskInfoViewModel
            {
                Id = taskId,
                TaskState = state,
            };
            var completeTaskResponse1 = await client.PostAsync(
                PerformerUrl("complete_task"), 
                new StringContent(JsonConvert.SerializeObject(completeTaskViewModel3), Encoding.UTF8, ContentType));
            Assert.Equal(expectedCode, completeTaskResponse1.StatusCode);
        }
        
        private static async Task TryCancelByManager(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var inWorkViewModel1 = new TaskInfoViewModel
            {
                Id = taskId,
            };
            var inWorkResponse1 = await client.PostAsync(
                ManagerUrl("cancel_task"), 
                new StringContent(JsonConvert.SerializeObject(inWorkViewModel1), Encoding.UTF8, ContentType));
            Assert.Equal(expectedCode, inWorkResponse1.StatusCode);
        }
        
        #endregion
        
        #region TestOneUndistributedTask

        private static async Task CheckOneUndistributedTask(
            HttpClient clientManager1,
            HttpClient clientManager2,
            HttpClient clientPerformer1,
            HttpClient clientPerformer2,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            string firstExpectedState,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            string secondExpectedState)
        {
            // Смотрим задания за менеджеров
            var response1 = await clientManager1.GetAsync(ManagerUrl("my_tasks"));
            var tasksList1 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().TaskState);
            Assert.Equal("TaskCaption", tasksList1.First().Caption);
            Assert.Equal("TaskContent", tasksList1.First().ContentPreview);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList2.Length);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(firstExpectedState, tasksList3.First().TaskState);
            Assert.Equal("TaskCaption", tasksList3.First().Caption);
            Assert.Equal("TaskContent", tasksList3.First().ContentPreview);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList4.Length);
            Assert.Equal(secondExpectedState, tasksList4.First().TaskState);
            Assert.Equal("TaskCaption", tasksList4.First().Caption);
            Assert.Equal("TaskContent", tasksList4.First().ContentPreview);
        }
        
        private static async Task CheckOneTask(
            HttpClient clientManager1,
            HttpClient clientManager2,
            HttpClient clientPerformer1,
            HttpClient clientPerformer2,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            string firstExpectedState)
        {
            // Смотрим задания за менеджеров
            var response1 = await clientManager1.GetAsync(ManagerUrl("my_tasks"));
            var tasksList1 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().TaskState);
            Assert.Equal("TaskCaption", tasksList1.First().Caption);
            Assert.Equal("TaskContent", tasksList1.First().ContentPreview);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList2.Length);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(firstExpectedState, tasksList3.First().TaskState);
            Assert.Equal("TaskCaption", tasksList3.First().Caption);
            Assert.Equal("TaskContent", tasksList3.First().ContentPreview);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList4.Length);
        }
        
        #endregion
        
        #region TestTwoTasksWithSpecifiedPerformers
        
        private static async Task CheckTasksForTwoPerformers(
            HttpClient clientManager1,
            HttpClient clientManager2,
            HttpClient clientPerformer1,
            HttpClient clientPerformer2,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            string firstExpectedState,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            string secondExpectedState = null)
        {
            // Смотрим задания за менеджеров
            var response1 = await clientManager1.GetAsync(ManagerUrl("my_tasks"));
            var tasksList1 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().TaskState);
            Assert.Equal("TaskCaption1", tasksList1.First().Caption);
            Assert.Equal("TaskContent1", tasksList1.First().ContentPreview);
            var firstTaskId = tasksList1.First().Id;
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList2.Length);
            Assert.Equal(secondExpectedState ?? firstExpectedState, tasksList2.First().TaskState);
            Assert.Equal("TaskCaption2", tasksList2.First().Caption);
            Assert.Equal("TaskContent2", tasksList2.First().ContentPreview);
            var secondTaskId = tasksList2.First().Id;
            
            // Смотрим нераспределенные за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList3.Length);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList4.Length);
            
            // Смотрим задания за исполнителей
            var response5 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList5 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response5.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList5.Length);
            Assert.Equal(firstTaskId, tasksList5.First().Id);
            Assert.Equal(firstExpectedState, tasksList5.First().TaskState);
            Assert.Equal("TaskCaption1", tasksList5.First().Caption);
            Assert.Equal("TaskContent1", tasksList5.First().ContentPreview);
            var response6 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList6 =  
                JsonConvert.DeserializeObject<TaskPreviewViewModel[]>(await response6.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList6.Length);
            Assert.Equal(secondTaskId, tasksList6.First().Id);
            Assert.Equal(secondExpectedState ?? firstExpectedState, tasksList6.First().TaskState);
            Assert.Equal("TaskCaption2", tasksList6.First().Caption);
            Assert.Equal("TaskContent2", tasksList6.First().ContentPreview);

        }

        #endregion
        
    }
}