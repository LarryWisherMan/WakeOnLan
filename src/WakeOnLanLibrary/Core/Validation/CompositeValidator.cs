using System;
using System.Collections.Generic;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;

namespace WakeOnLanLibrary.Core.Validators
{
    /// <summary>
    /// A composite validator that executes a collection of validation strategies for a specific type.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    public class CompositeValidator<T> where T : Computer
    {
        private readonly IReadOnlyList<IValidationStrategy<T>> _validationStrategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeValidator{T}"/> class.
        /// </summary>
        /// <param name="validationStrategies">A collection of validation strategies to execute.</param>
        public CompositeValidator(IEnumerable<IValidationStrategy<T>> validationStrategies)
        {
            if (validationStrategies == null)
            {
                throw new ArgumentNullException(nameof(validationStrategies), "Validation strategies cannot be null.");
            }

            _validationStrategies = new List<IValidationStrategy<T>>(validationStrategies);
        }

        /// <summary>
        /// Executes all validation strategies and returns the first failed validation result, or success if all pass.
        /// </summary>
        /// <param name="entity">The object to validate.</param>
        /// <returns>The first failed <see cref="ValidationResult"/>, or success if all validations pass.</returns>
        public ValidationResult Validate(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity to validate cannot be null.");
            }

            foreach (var strategy in _validationStrategies)
            {
                var result = strategy.Validate(entity);
                if (!result.IsValid)
                {
                    // Return the exact ValidationResult from the failing strategy
                    return result;
                }
            }

            // If all validations pass, return a success result from the last strategy
            return new ValidationResult { IsValid = true, Message = "All validations passed." };
        }

        /// <summary>
        /// Executes all validation strategies and returns all results, including both successes and failures.
        /// </summary>
        /// <param name="entity">The object to validate.</param>
        /// <returns>A list of all <see cref="ValidationResult"/>s from the validation strategies.</returns>
        public IEnumerable<ValidationResult> ValidateAll(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity to validate cannot be null.");
            }

            var results = new List<ValidationResult>();

            foreach (var strategy in _validationStrategies)
            {
                var result = strategy.Validate(entity);
                results.Add(result);
            }

            return results;
        }
    }
}