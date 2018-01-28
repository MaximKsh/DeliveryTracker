using DeliveryTracker.Common;

namespace DeliveryTracker.References
{
    public class ReferenceDescription : DictionaryObject
    {
        public string Caption
        {
            get => this.Get<string>(nameof(this.Caption));
            set => this.Set(nameof(this.Caption), value);
        }
    }
}