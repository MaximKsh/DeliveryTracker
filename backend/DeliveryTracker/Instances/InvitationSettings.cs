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
            int expires,
            int codeLength,
            string alphabet)
        {
            this.Name = name;
            this.Expires = expires;
            this.CodeLength = codeLength;
            this.Alphabet = alphabet;
        }

        /// <inheritdoc /> 
        public string Name { get; }
        
        /// <summary>
        /// Срок истечения приглашения в минутах
        /// </summary>
        public int Expires { get; }

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