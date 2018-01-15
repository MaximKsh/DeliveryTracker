namespace DeliveryTrackerTest.Controllers
{
    public class WorkflowTest: BaseControllerTest
    {
        /*
        /// <summary>
        /// Создаются 10 исполнителей.
        /// Смотрим что они возвращаются в списке
        /// </summary>
        [Fact]
        public async void TestAvailablePerformers()
        {
            var client = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(client, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(client, creator.Username, "123qQ!");
            var performersUsernames = await MassCreateUsers(client, token.Token, RoleAlias.Performer, "123qQ!", 10);
            var manager = (await MassCreateUsers(client, token.Token, RoleAlias.Manager, "123qQ!", 1)).First();
            var managerToken = (await GetToken(client, manager, "123qQ!")).Token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken);
            
            // Смотрим активных
            var response1 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList1 =  
                JsonConvert.DeserializeObject<UserViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList1.Length);
            
            // Ставим каждому нечетному 
            for (var i = 0; i < performersUsernames.Count; i += 2)
            {
                var perfClient = this.Server.CreateClient();
                var perfToken = await GetToken(client, performersUsernames[i], "123qQ!");
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken.Token);
                await UpdateRandomPosition(perfClient);
            }
            
            // Смотрим активных
            var response2 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList2 =  
                JsonConvert.DeserializeObject<UserViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList2.Length);
            
            // Ставим каждому нечетному 
            for (var i = 0; i < performersUsernames.Count; i += 2)
            {
                var perfClient = this.Server.CreateClient();
                var perfToken = await GetToken(client, performersUsernames[i], "123qQ!");
                perfClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", perfToken.Token);
                await SetInactive(perfClient);
            }
            
            // Смотрим активных
            var response3 = await client.GetAsync(ManagerUrl("available_performers"));
            var performersList3 =  
                JsonConvert.DeserializeObject<UserViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(10, performersList3.Length);
            
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
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performers = await MassCreateUsers(clientManager1, token.Token, RoleAlias.Performer, "123qQ!", 2);
            var managers = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Manager, "123qQ!", 2));
            
            var managerToken1 = await GetToken(clientManager1, managers.First(), "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1.Token);
            var managerToken2 = await GetToken(clientManager2, managers.Last(), "123qQ!");
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2.Token);
            
            var performerToken1 = await GetToken(clientPerformer1, performers.First(), "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1.Token);
            var performerToken2 = await GetToken(clientPerformer2, performers.Last(), "123qQ!");
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2.Token);
            
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
                TaskStateAlias.NewUndistributedState,
                TaskStateAlias.NewUndistributedState);
            
            // Резервируем за первого исполнителя
            await TryReserve(clientPerformer1, taskId);
            
            // Берем в работу за первого исполнителя
            await TryTakeIntoWork(clientPerformer1, taskId);
            
            await CheckOneTask(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateAlias.InWorkState);
            
            // Смотрим задания за исполнителей по нераспределенным
            var response7 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList7 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response7.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList7.Length);
            var response8 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList8 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response8.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList8.Length);
            
            // Пытаемся завершить за второго
            await TryCompleteTask(clientPerformer2, taskId, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            
            // Завершаем задание за первого 
            await TryCompleteTask(clientPerformer1, taskId, TaskStateAlias.PerformedState);
            
            await CheckOneTask(
                clientManager1,
                clientManager2,
                clientPerformer1,
                clientPerformer2,
                TaskStateAlias.PerformedState);
            
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
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performers = await MassCreateUsers(clientManager1, token.Token, RoleAlias.Performer, "123qQ!", 2);
            var managers = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Manager, "123qQ!", 2));
            
            var managerToken1 = await GetToken(clientManager1, managers.First(), "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1.Token);
            var managerToken2 = await GetToken(clientManager2, managers.Last(), "123qQ!");
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2.Token);
            
            var performerToken1 = await GetToken(clientPerformer1, performers.First(), "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1.Token);
            var performerToken2 = await GetToken(clientPerformer2, performers.Last(), "123qQ!");
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2.Token);
            
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
                TaskStateAlias.NewState);
            
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
                TaskStateAlias.InWorkState);
            
            // Пытаемся завершать чужое задание
            await TryCompleteTask(clientPerformer1, secondTaskId, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientManager2, firstTaskId, TaskStateAlias.CancelledState, HttpStatusCode.Forbidden);
            
            // Завершаем первое задание за первого 
            await TryCompleteTask(clientPerformer1, firstTaskId, TaskStateAlias.PerformedState);
            
            // Отменяем второе задание за второго
            await TryCompleteTask(clientPerformer2, secondTaskId, TaskStateAlias.CancelledState);
            
            
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
        public async void TestTwoInstances()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientManager2 = this.Server.CreateClient();
            
            var clientPerformer1 = this.Server.CreateClient();
            var clientPerformer2 = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token1 = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performers1 = await MassCreateUsers(clientManager1, token1.Token, RoleAlias.Performer, "123qQ!", 1);
            var managers1 = (await MassCreateUsers(clientManager1, token1.Token, RoleAlias.Manager, "123qQ!", 1));
            
            var creator2 = 
                await CreateInstance(clientManager2, "Иванов И.И.", "123qQ!", "Instance1");
            var token2 = await GetToken(clientManager2, creator2.Username, "123qQ!");
            var performers2 = await MassCreateUsers(clientManager2, token2.Token, RoleAlias.Performer, "123qQ!", 1);
            var managers2 = (await MassCreateUsers(clientManager2, token2.Token, RoleAlias.Manager, "123qQ!", 1));
            
            var managerToken1 = await GetToken(clientManager1, managers1.First(), "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken1.Token);
            var managerToken2 = await GetToken(clientManager2, managers2.First(), "123qQ!");
            clientManager2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken2.Token);
            
            var performerToken1 = await GetToken(clientPerformer1, performers1.First(), "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken1.Token);
            var performerToken2 = await GetToken(clientPerformer2, performers2.First(), "123qQ!");
            clientPerformer2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken2.Token);
           
            
            // Поставим исполнителя в работу
            await UpdateRandomPosition(clientPerformer1);
            await UpdateRandomPosition(clientPerformer2);

            // Создаем таск
            var taskId11 = await AddTask(clientManager1, "TaskCaption11", "TaskContent11");
            var taskId12 = await AddTask(clientManager1, "TaskCaption12", "TaskContent12", performers1.First());
            var taskId21 = await AddTask(clientManager2, "TaskCaption21", "TaskContent21");
            var taskId22 = await AddTask(clientManager2, "TaskCaption22", "TaskContent22", performers2.First());
            
            await AddTask(clientManager1, "1", "1", performers2.First(), 
                expectedStatus: HttpStatusCode.Forbidden, errorCode:ErrorCode.PerformerInAnotherInstance);
            await AddTask(clientManager2, "1", "1", performers1.First(),
                expectedStatus: HttpStatusCode.Forbidden, errorCode:ErrorCode.PerformerInAnotherInstance);
                
            // Смотрим задания за менеджеров
            var response1 = await clientManager1.GetAsync(ManagerUrl("my_tasks"));
            var tasksList1 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(2, tasksList1.Length);
            Assert.Equal(TaskStateAlias.NewUndistributedState, tasksList1.First().State);
            Assert.Equal(TaskStateAlias.NewState, tasksList1.Last().State);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(2, tasksList2.Length);
            Assert.Equal(TaskStateAlias.NewUndistributedState, tasksList2.First().State);
            Assert.Equal(TaskStateAlias.NewState, tasksList2.Last().State);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(TaskStateAlias.NewUndistributedState, tasksList3.First().State);
            var response4 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList4.Length);
            Assert.Equal(TaskStateAlias.NewState, tasksList4.First().State);
            
            var response5 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList5 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response5.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList5.Length);
            Assert.Equal(TaskStateAlias.NewUndistributedState, tasksList5.First().State);
            
            var response6 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList6 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response6.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList6.Length);
            Assert.Equal(TaskStateAlias.NewState, tasksList6.First().State);
            
            
            // Пытаемся взять чужие задания в работу
            await TryTakeIntoWork(clientPerformer1, taskId21, HttpStatusCode.Forbidden);
            await TryTakeIntoWork(clientPerformer1, taskId22, HttpStatusCode.Forbidden);
            
            await TryTakeIntoWork(clientPerformer2, taskId11, HttpStatusCode.Forbidden);
            await TryTakeIntoWork(clientPerformer2, taskId12, HttpStatusCode.Forbidden);

            // Загружаем и смотрим что ничего не поменялось
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateAlias.NewState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateAlias.NewState);
            
            // Берем свои задания в работу
            await TryReserve(clientPerformer1, taskId11);
            await TryTakeIntoWork(clientPerformer1, taskId11);
            await TryTakeIntoWork(clientPerformer1, taskId12);
            
            await TryReserve(clientPerformer2, taskId21);
            await TryTakeIntoWork(clientPerformer2, taskId21);
            await TryTakeIntoWork(clientPerformer2, taskId22);
            
            // Пробуем загружать чужие таски
            await GetTaskByPerformer(clientPerformer1, taskId21, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer1, taskId22, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer2, taskId11, HttpStatusCode.NotFound);
            await GetTaskByPerformer(clientPerformer2, taskId12, HttpStatusCode.NotFound);

            // Пробуем завершить чужие таски
            await TryCompleteTask(clientPerformer1, taskId21, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer1, taskId22, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer2, taskId11, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            await TryCompleteTask(clientPerformer2, taskId12, TaskStateAlias.PerformedState, HttpStatusCode.Forbidden);
            
            // Проверяем что ничего не поменялось
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateAlias.InWorkState);
            
            // Завершаем свои
            await TryCompleteTask(clientPerformer1, taskId11, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer1, taskId12, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer2, taskId21, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer2, taskId22, TaskStateAlias.PerformedState);
            
            // Проверяем что завершились
            await CheckTaskStateByPerformer(clientPerformer1, taskId11, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskId12, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId21, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer2, taskId22, TaskStateAlias.PerformedState);
        }


        /// <summary>
        /// Отменяются менеджером задания во всех состояниях.
        /// </summary>
        [Fact]
        public async void CancellingTaskByManager()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performer = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Performer, "123qQ!", 1))[0];
            var manager = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Manager, "123qQ!", 1))[0];
            
            var managerToken = await GetToken(clientManager1, manager, "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken.Token);
            
            var performerToken = await GetToken(clientPerformer1, performer, "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken.Token);
            
            var taskIUndistributed = await AddTask(clientManager1, "1", "1");
            var taskIdNew = await AddTask(clientManager1, "1", "1", performer);
            var taskIdInWork = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);

            await TryTakeIntoWork(clientPerformer1, taskIdInWork);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);

            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateAlias.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            
            await CheckTaskStateByManager(clientManager1, taskIUndistributed, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByManager(clientManager1, taskIdNew, TaskStateAlias.NewState);
            await CheckTaskStateByManager(clientManager1, taskIdInWork, TaskStateAlias.InWorkState);
            await CheckTaskStateByManager(clientManager1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByManager(clientManager1, taskIdCancelled, TaskStateAlias.CancelledState);

            await TryCancelByManager(clientManager1, taskIUndistributed);
            await TryCancelByManager(clientManager1, taskIdNew);
            await TryCancelByManager(clientManager1, taskIdInWork);
            await TryCancelByManager(clientManager1, taskIdPerformed, HttpStatusCode.BadRequest);
            await TryCancelByManager(clientManager1, taskIdCancelled, HttpStatusCode.BadRequest);
            
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            
            await CheckTaskStateByManager(clientManager1, taskIUndistributed, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdNew, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdInWork, TaskStateAlias.CancelledByManagerState);
            await CheckTaskStateByManager(clientManager1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByManager(clientManager1, taskIdCancelled, TaskStateAlias.CancelledState);
        }


        /// <summary>
        /// Попытка завершать задания не в работе (новые или завершенные)
        /// </summary>
        [Fact]
        public async void CompleteNotInWorkTasks()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performer = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Performer, "123qQ!", 1))[0];
            var manager = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Manager, "123qQ!", 1))[0];
            
            var managerToken = await GetToken(clientManager1, manager, "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken.Token);
            
            var performerToken = await GetToken(clientPerformer1, performer, "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken.Token);
            
            var taskIUndistributed = await AddTask(clientManager1, "1", "1");
            var taskIdNew = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelledByManager = await AddTask(clientManager1, "1", "1", performer);
            
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await TryCancelByManager(clientManager1, taskIdCancelledByManager);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateAlias.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.CancelledByManagerState);
            
            await TryCompleteTask(clientPerformer1, taskIUndistributed, TaskStateAlias.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIUndistributed, TaskStateAlias.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdNew, TaskStateAlias.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdNew, TaskStateAlias.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateAlias.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateAlias.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.PerformedState, HttpStatusCode.BadRequest);
            await TryCompleteTask(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.CancelledState, HttpStatusCode.BadRequest);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIUndistributed, TaskStateAlias.NewUndistributedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdNew, TaskStateAlias.NewState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.CancelledByManagerState);
        }

        [Fact]
        public async void InWorkNotNewTasks()
        {
            var clientManager1 = this.Server.CreateClient();
            var clientPerformer1 = this.Server.CreateClient();
            
            var creator = 
                await CreateInstance(clientManager1, "Иванов И.И.", "123qQ!", "Instance1");
            var token = await GetToken(clientManager1, creator.Username, "123qQ!");
            var performer = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Performer, "123qQ!", 1))[0];
            var manager = (await MassCreateUsers(clientManager1, token.Token, RoleAlias.Manager, "123qQ!", 1))[0];
            
            var managerToken = await GetToken(clientManager1, manager, "123qQ!");
            clientManager1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken.Token);
            
            var performerToken = await GetToken(clientPerformer1, performer, "123qQ!");
            clientPerformer1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", performerToken.Token);
            
            var taskIdInWork = await AddTask(clientManager1, "1", "1", performer);
            var taskIdPerformed = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelled = await AddTask(clientManager1, "1", "1", performer);
            var taskIdCancelledByManager = await AddTask(clientManager1, "1", "1", performer);
            
            await TryTakeIntoWork(clientPerformer1, taskIdInWork);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled);
            await TryCompleteTask(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await TryCompleteTask(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await TryCancelByManager(clientManager1, taskIdCancelledByManager);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.CancelledByManagerState);

            await TryTakeIntoWork(clientPerformer1, taskIdInWork, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdPerformed, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelled, HttpStatusCode.BadRequest);
            await TryTakeIntoWork(clientPerformer1, taskIdCancelledByManager, HttpStatusCode.BadRequest);
            
            await CheckTaskStateByPerformer(clientPerformer1, taskIdInWork, TaskStateAlias.InWorkState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdPerformed, TaskStateAlias.PerformedState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelled, TaskStateAlias.CancelledState);
            await CheckTaskStateByPerformer(clientPerformer1, taskIdCancelledByManager, TaskStateAlias.CancelledByManagerState);
            
        }
        
        #region common

        private static async Task CheckTaskStateByManager(HttpClient client, Guid taskId, string state)
        {
            var task1 = await GetTaskByManager(client, taskId);
            Assert.Equal(state, task1.State);
        }
        
        private static async Task CheckTaskStateByPerformer(HttpClient client, Guid taskId, string state)
        {
            var task1 = await GetTaskByPerformer(client, taskId);
            Assert.Equal(state, task1.State);
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
            string number, 
            string shippingList,
            string performer = null,
            HttpStatusCode expectedStatus = HttpStatusCode.Created,
            string errorCode = null)
        {
            var taskViewModel = new TaskViewModel
            {
                Number = number,
                ShippingDesc = shippingList,
                Performer = new UserViewModel
                {
                    Username = performer
                },
            };
            var addTaskResponse = await client.PostAsync(
                ManagerUrl("add_task"), 
                new StringContent(JsonConvert.SerializeObject(taskViewModel), Encoding.UTF8, ContentType));
            Assert.Equal(expectedStatus, addTaskResponse.StatusCode);
            if (expectedStatus == HttpStatusCode.Created)
            {
                var task =
                    JsonConvert.DeserializeObject<TaskViewModel>(await addTaskResponse.Content.ReadAsStringAsync());
                var expectingState = performer != null
                    ? TaskStateAlias.NewState
                    : TaskStateAlias.NewUndistributedState;
                Assert.Equal(expectingState, task.State);
                return task.Id ?? Guid.Empty;
            }
            else
            {
                var task =
                    JsonConvert.DeserializeObject<ErrorListViewModel>(await addTaskResponse.Content.ReadAsStringAsync());
                Assert.Equal(errorCode, task.Errors.First().Code);
                return Guid.Empty;
            }
        }

        private static async Task<TaskViewModel> GetTaskByManager(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var taskResponse = await client.GetAsync(ManagerUrl($"task/{taskId}"));
            Assert.Equal(expectedCode, taskResponse.StatusCode);

            return expectedCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<TaskViewModel>(await taskResponse.Content.ReadAsStringAsync()) 
                : null;
        }
        
        
        private static async Task<TaskViewModel> GetTaskByPerformer(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var taskResponse = await client.GetAsync(PerformerUrl($"task/{taskId}"));
            Assert.Equal(expectedCode, taskResponse.StatusCode);

            return expectedCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<TaskViewModel>(await taskResponse.Content.ReadAsStringAsync()) 
                : null;
        }
        
        private static async Task TryReserve(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var inWorkViewModel1 = new TaskViewModel
            {
                Id = taskId,
            };
            var inWorkResponse1 = await client.PostAsync(
                PerformerUrl("reserve_task"), 
                new StringContent(JsonConvert.SerializeObject(inWorkViewModel1), Encoding.UTF8, ContentType));
            Assert.Equal(expectedCode, inWorkResponse1.StatusCode);
        }
        
        private static async Task TryTakeIntoWork(
            HttpClient client,
            Guid taskId,
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            var inWorkViewModel1 = new TaskViewModel
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
            var completeTaskViewModel3 = new TaskViewModel
            {
                Id = taskId,
                State = state,
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
            var inWorkViewModel1 = new TaskViewModel
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
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().State);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList2.Length);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(firstExpectedState, tasksList3.First().State);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList4.Length);
            Assert.Equal(secondExpectedState, tasksList4.First().State);
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
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().State);
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList2.Length);
            
            // Смотрим задания за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList3.Length);
            Assert.Equal(firstExpectedState, tasksList3.First().State);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response4.Content.ReadAsStringAsync());
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
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response1.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList1.Length);
            Assert.Equal(firstExpectedState, tasksList1.First().State);
            var firstTaskId = tasksList1.First().Id;
            
            var response2 = await clientManager2.GetAsync(ManagerUrl("my_tasks"));
            var tasksList2 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response2.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList2.Length);
            Assert.Equal(secondExpectedState ?? firstExpectedState, tasksList2.First().State);
            var secondTaskId = tasksList2.First().Id;
            
            // Смотрим нераспределенные за исполнителей
            var response3 = await clientPerformer1.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList3 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response3.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList3.Length);
            var response4 = await clientPerformer2.GetAsync(PerformerUrl("undistributed_tasks"));
            var tasksList4 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response4.Content.ReadAsStringAsync());
            Assert.Equal(0, tasksList4.Length);
            
            // Смотрим задания за исполнителей
            var response5 = await clientPerformer1.GetAsync(PerformerUrl("my_tasks"));
            var tasksList5 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response5.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList5.Length);
            Assert.Equal(firstTaskId, tasksList5.First().Id);
            Assert.Equal(firstExpectedState, tasksList5.First().State);
            var response6 = await clientPerformer2.GetAsync(PerformerUrl("my_tasks"));
            var tasksList6 =  
                JsonConvert.DeserializeObject<TaskViewModel[]>(await response6.Content.ReadAsStringAsync());
            Assert.Equal(1, tasksList6.Length);
            Assert.Equal(secondTaskId, tasksList6.First().Id);
            Assert.Equal(secondExpectedState ?? firstExpectedState, tasksList6.First().State);

        }

        #endregion
        */
    }
}