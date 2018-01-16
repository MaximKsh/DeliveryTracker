using System;
using System.Collections.Generic;
using DeliveryTracker.Validation;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace DeliveryTracker.Tests.Validation
{
    public class ParametersValidatorTest
    {
        
        
        [Fact]
        public void Correct()
        {
            // Arrange
            var v1 = 5;
            var v2 = new object();
            var v3 = Guid.NewGuid();
            var v4 = "123";

            // Act
            var result = new ParametersValidator()
                .AddRule("v1", v1, x => x > 3)
                .AddNotNullRule("v2", v2)
                .AddNotEmptyGuidRule("v3", v3)
                .AddNotNullOrWhitespaceRule("v4", v4)
                .Validate();

            // Assert
            Assert.True(result.Success);
        }

        [Theory]
        [MemberData(nameof(WrongCombinations))]
        public void Wrong(string name, object value, Func<dynamic, bool> action)
        {
            // Act
            var result = new ParametersValidator()
                .AddRule(name, value, action)
                .Validate();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(
                Enumerable.AsEnumerable(result.Error.Info),
                x => x.Key == name && x.Value == (value?.ToString() ?? "null"));
        }

        [Fact]
        public void UseBuiltValidator()
        {
            // Arrange
            var v1 = 5;

            // Act
            var validator = new ParametersValidator()
                .AddRule("v1", v1, x => x > 3);
            validator.Validate();
            
            // Assert
            Assert.Throws<InvalidOperationException>(() => validator.Validate());

        }
        
        public static IEnumerable<object[]> WrongCombinations()
        {
            yield return new object[] {"v1", null, (Func<dynamic, bool>)(x => x != null)};
            yield return new object[] {"v2", null, (Func<dynamic, bool>)(x => x != null && !string.IsNullOrWhiteSpace(x))};
            yield return new object[] {"v3", "", (Func<dynamic, bool>)(x => x != null && !string.IsNullOrWhiteSpace(x))};
            yield return new object[] {"v4", Guid.Empty, (Func<dynamic, bool>)(x => x != Guid.Empty)};
            yield return new object[] {"v5", 5, (Func<dynamic, bool>)(x => x > 10)};
            yield return new object[] {"v6", "Hello, World!", (Func<dynamic, bool>)(x => x.StartsWith("Goodbye"))};
        }
        
    }
}