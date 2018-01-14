using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryTracker.Validation
{
    public class ParametersValidator
    {
        private class Rule
        {
            public string VariableName;
            public dynamic Value;
            public Func<dynamic, bool> Predicate;
        }

        private bool wasBuilded = false;
        
        private readonly List<Rule> rules = new List<Rule>();

        public ParametersValidator AddRule(
            string variableName,
            dynamic value,
            Func<dynamic, bool> predicate)
        {
            if (this.wasBuilded)
            {
                throw new InvalidOperationException("ParametersValidator was built.");
            }
            
            this.rules.Add(new Rule
            {
                VariableName = variableName,
                Value = value,
                Predicate = predicate,
            });
            return this;
        }

        public ParametersValidationResult Validate()
        {
            this.wasBuilded = true;
            var negativeRules = this.rules
                .Where(p => !p.Predicate(p.Value))
                .Select(p => new KeyValuePair<string, object>(p.VariableName, p.Value))
                .ToList();
            return negativeRules.Any()
                ? new ParametersValidationResult(ErrorFactory.ValidationError(negativeRules)) 
                : new ParametersValidationResult();
        }
        
    }
}