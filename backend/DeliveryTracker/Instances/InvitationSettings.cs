namespace DeliveryTracker.Instances
{
    public sealed class InvitationSettings
    {
        public InvitationSettings(
            int expiresInDays,
            int codeLength,
            string alphabet)
        {
            this.ExpiresInDays = expiresInDays;
            this.CodeLength = codeLength;
            this.Alphabet = alphabet;
        }
        
        public int ExpiresInDays { get; }

        public int CodeLength { get; }

        public string Alphabet { get; }
    }
}