namespace DeliveryTracker.Instances
{
    /// <summary>
    /// Настройки для приглашений.
    /// </summary>
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
        
        /// <summary>
        /// Срок истечения приглашения в днях
        /// </summary>
        public int ExpiresInDays { get; }

        /// <summary>
        /// Длина кода приглашения.
        /// </summary>
        public int CodeLength { get; }

        /// <summary>
        /// Символы, из которых может состоять код приглашения.
        /// </summary>
        public string Alphabet { get; }
    }
}