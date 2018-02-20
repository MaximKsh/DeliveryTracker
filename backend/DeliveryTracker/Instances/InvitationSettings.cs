using DeliveryTracker.Common;

namespace DeliveryTracker.Instances
{
    /// <summary>
    /// Настройки для приглашений.
    /// </summary>
    public sealed class InvitationSettings : ISettings
    {
        public InvitationSettings(
            string name,
            int expiresInDays,
            int codeLength,
            string alphabet)
        {
            this.Name = name;
            this.ExpiresInDays = expiresInDays;
            this.CodeLength = codeLength;
            this.Alphabet = alphabet;
        }

        /// <inheritdoc /> 
        public string Name { get; }
        
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