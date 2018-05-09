using System;
using DeliveryTracker.Common;

namespace DeliveryTracker.Statistics
{
    /// <summary>
    /// Элемент статистики.
    /// </summary>
    public sealed class TaskStatisticsItem : DictionaryObject
    {
        public string Key
        {
            get => this.Get<string>(nameof(this.Key));
            set => this.Set(nameof(this.Key), value);
        }
        
        public DateTime? DatePoint
        {
            get => this.Get<DateTime>(nameof(this.DatePoint));
            set => this.Set(nameof(this.DatePoint), value);
        }

        public int Created 
        {
            get => this.Get<int>(nameof(this.Created));
            set => this.Set(nameof(this.Created), value);
        }
        
        public int Completed 
        {
            get => this.Get<int>(nameof(this.Completed));
            set => this.Set(nameof(this.Completed), value);
        }
        
        public int Preparing 
        {
            get => this.Get<int>(nameof(this.Preparing));
            set => this.Set(nameof(this.Preparing), value);
        }
        
        public int Queue 
        {
            get => this.Get<int>(nameof(this.Queue));
            set => this.Set(nameof(this.Queue), value);
        }

        public int Waiting 
        {
            get => this.Get<int>(nameof(this.Waiting));
            set => this.Set(nameof(this.Waiting), value);
        }
        
        public int IntoWork
        {
            get => this.Get<int>(nameof(this.IntoWork));
            set => this.Set(nameof(this.IntoWork), value);
        }
        
        public int Delivered
        {
            get => this.Get<int>(nameof(this.Delivered));
            set => this.Set(nameof(this.Delivered), value);
        }
        
        public int Complete
        {
            get => this.Get<int>(nameof(this.Complete));
            set => this.Set(nameof(this.Complete), value);
        }
        
        public int Revoked 
        {
            get => this.Get<int>(nameof(this.Revoked));
            set => this.Set(nameof(this.Revoked), value);
        }
        
    }
}