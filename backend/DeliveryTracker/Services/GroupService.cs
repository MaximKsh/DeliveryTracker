using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DeliveryTracker.Db;
using DeliveryTracker.Models;
using DeliveryTracker.ViewModels;

namespace DeliveryTracker.Services
{
    public class GroupService
    {

        private readonly DeliveryTrackerDbContext dbContext;
        
        public GroupService(DeliveryTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<GroupModel> CreateGroup(string name)
        {
            var group = new GroupModel
            {
                Id = Guid.NewGuid(),
                DisplayableName = name,
            };
            group = (await this.dbContext.Groups.AddAsync(group)).Entity;
            return group;
        }

        public void SetCreator(GroupModel group, UserModel creator)
        {
            group.CreatorId = creator.Id;
            this.dbContext.Groups.Update(group);
        }
        
        
        
    }
}