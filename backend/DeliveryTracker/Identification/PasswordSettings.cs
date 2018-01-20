using System.Collections.Immutable;

namespace DeliveryTracker.Identification
{
    /// <summary>
    /// Настройки корректности пароля
    /// </summary>
    public sealed class PasswordSettings
    {
        public PasswordSettings(
            int minLength,
            int maxLength,
            bool atLeastOneUpperCase, 
            bool atLeastOneLowerCase,
            bool atLeastOneDigit, 
            string alphabet,
            int sameCharactersInARow)
        {
            this.MinLength = minLength;
            this.MaxLength = maxLength;
            this.AtLeastOneUpperCase = atLeastOneUpperCase;
            this.AtLeastOneLowerCase = atLeastOneLowerCase;
            this.AtLeastOneDigit = atLeastOneDigit;
            this.Alphabet = alphabet.ToImmutableHashSet();
            this.HasAlphabet = alphabet.Length != 0;
            this.SameCharactersInARow = sameCharactersInARow;
        }
    
        /// <summary>
        /// Минимальная длина пароля.
        /// </summary>
        public int MinLength { get; }
        
        /// <summary>
        /// Максимальная длина пароля.
        /// </summary>
        public int MaxLength { get; }
        
        /// <summary>
        /// В пароле должен присутствовать хотя бы один символ в верхнем регистре.
        /// </summary>
        public bool AtLeastOneUpperCase { get; }
        
        /// <summary>
        /// В пароле должен присутствовать хотя бы один символ в нижнем регистре.
        /// </summary>
        public bool AtLeastOneLowerCase { get; }
        
        /// <summary>
        /// В пароле должна быть хотя бы одна цифра.
        /// </summary>
        public bool AtLeastOneDigit { get; }
        
        /// <summary>
        /// Символы, из которых может состоять пароль.
        /// </summary>
        public ImmutableHashSet<char> Alphabet { get; }
        
        /// <summary>
        /// Есть ли ограничение на набор символов в пароле.
        /// Вычисляется из поля Alphabet
        /// </summary>
        public bool HasAlphabet { get; }
        
        /// <summary>
        /// Максимальное количество одинаковых символов подряд.
        /// </summary>
        public int SameCharactersInARow { get; }
        
    }
}