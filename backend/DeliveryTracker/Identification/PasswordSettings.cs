using System.Collections.Immutable;

namespace DeliveryTracker.Identification
{
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
    
        public int MinLength { get; }
        public int MaxLength { get; }
        public bool AtLeastOneUpperCase { get; }
        public bool AtLeastOneLowerCase { get; }
        public bool AtLeastOneDigit { get; }
        public ImmutableHashSet<char> Alphabet { get; }
        public bool HasAlphabet { get; }
        public int SameCharactersInARow { get; }
        
    }
}