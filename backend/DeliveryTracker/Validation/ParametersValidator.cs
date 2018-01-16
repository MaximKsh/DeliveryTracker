using System;
using System.Collections.Generic;

namespace DeliveryTracker.Validation
{
    /// <summary>
    /// Класс для проверки параметров по заданным правилам.
    /// </summary>
    public class ParametersValidator
    {
        #region nested types
        
        private sealed class Rule
        {
            public string VariableName;
            public dynamic Value;
            public Func<dynamic, bool> Predicate;
        }

        #endregion
        
        #region fields
        
        private bool built = false;
        
        private readonly List<Rule> rules = new List<Rule>();

        #endregion
        
        #region public methods
        
        /// <summary>
        /// Добавить правило с проверкой по предикату.
        /// </summary>
        /// <param name="variableName">
        /// Имя переменной.
        /// В случае ошибки будет ключом в IError.Info.
        /// </param>
        /// <param name="value">
        /// Значение ошибки. Передается в предикат.
        /// В случае ошибки будет значением в IError.Info.
        /// </param>
        /// <param name="predicate">
        /// Предикат для проверки.
        /// </param>
        /// <returns>this</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ParametersValidator AddRule(
            string variableName,
            dynamic value,
            Func<dynamic, bool> predicate)
        {
            if (this.built)
            {
                throw new InvalidOperationException("ParametersValidator was built.");
            }

            this.rules.Add(new Rule
            {
                VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName)),
                Value = value,
                Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate)),
            });
            return this;
        }

        
        /// <summary>
        /// Проверка на value != null
        /// </summary>
        /// <param name="variableName">
        /// Имя переменной.
        /// В случае ошибки будет ключом в IError.Info.
        /// </param>
        /// <param name="value">
        /// Значение ошибки. Передается в предикат.
        /// В случае ошибки будет значением в IError.Info.
        /// </param>
        /// <returns>this</returns>
        public ParametersValidator AddNotNullRule(
            string variableName,
            object value)
        {
            return this.AddRule(variableName, value, v => v != null);
        }

        /// <summary>
        /// Проверка на то, что строка не пустаяe
        /// </summary>
        /// <param name="variableName">
        /// Имя переменной.
        /// В случае ошибки будет ключом в IError.Info.
        /// </param>
        /// <param name="value">
        /// Значение ошибки. Передается в предикат.
        /// В случае ошибки будет значением в IError.Info.
        /// </param>
        /// <returns>this</returns>
        public ParametersValidator AddNotNullOrWhitespaceRule(
            string variableName,
            string value)
        {
            return this.AddRule(variableName, value, v => v != null && !string.IsNullOrWhiteSpace(value));
        }
        
        /// <summary>
        /// Проверка на то, что значение != Guid.Empty
        /// </summary>
        /// <param name="variableName">
        /// Имя переменной.
        /// В случае ошибки будет ключом в IError.Info.
        /// </param>
        /// <param name="value">
        /// Значение ошибки. Передается в предикат.
        /// В случае ошибки будет значением в IError.Info.
        /// </param>
        /// <returns>this</returns>
        public ParametersValidator AddNotEmptyGuidRule(
            string variableName,
            Guid value)
        {
            return this.AddRule(variableName, value, v => value != Guid.Empty);
        }
        
        /// <summary>
        /// Выполнить валидацию по заданным правилам.
        /// </summary>
        /// <returns></returns>
        public ParametersValidationResult Validate()
        {
            if (this.built)
            {
                throw new InvalidOperationException("ParametersValidator was built.");
            }
            
            this.built = true;
            var negativeRules = new List<KeyValuePair<string, object>>(this.rules.Count);
            foreach (var p in this.rules)
            {
                if (!p.Predicate(p.Value))
                {
                    negativeRules.Add(new KeyValuePair<string, object>(p.VariableName, p.Value));
                }
            }
            return negativeRules.Count != 0
                ? new ParametersValidationResult(ErrorFactory.ValidationError(negativeRules)) 
                : new ParametersValidationResult();
        }
        
        #endregion
        
    }
}