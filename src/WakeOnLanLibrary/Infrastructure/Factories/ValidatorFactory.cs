using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    /// <summary>
    /// Factory for creating and retrieving validators for specific computer types.
    /// </summary>
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public CompositeValidator<TComputer> GetValidator<TComputer>() where TComputer : Computer
        {
            // Retrieve all validation strategies for the specific type
            var strategies = _serviceProvider
                .GetServices<IValidationStrategy<TComputer>>()
                .ToList();

            if (!strategies.Any())
            {
                throw new InvalidOperationException($"No validation strategies registered for type {typeof(TComputer).Name}.");
            }

            // Create and return a composite validator
            return new CompositeValidator<TComputer>(strategies);
        }
    }
}
