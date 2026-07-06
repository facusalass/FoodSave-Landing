using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Foodsave.Web.Infrastructure
{
    public class InvariantDecimalModelBinderProvider : IModelBinderProvider
    {
        private static readonly IModelBinder Binder =
            new InvariantDecimalModelBinder();

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var type = Nullable.GetUnderlyingType(context.Metadata.ModelType)
                       ?? context.Metadata.ModelType;

            return type == typeof(decimal) ? Binder : null;
        }
    }

    public class InvariantDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueResult =
                bindingContext.ValueProvider.GetValue(
                    bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(
                bindingContext.ModelName,
                valueResult);

            var value = valueResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                if (Nullable.GetUnderlyingType(
                        bindingContext.ModelMetadata.ModelType) is not null)
                {
                    bindingContext.Result =
                        ModelBindingResult.Success(null);
                }

                return Task.CompletedTask;
            }

            if (decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                bindingContext.Result =
                    ModelBindingResult.Success(parsed);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Ingresá un monto válido.");

            return Task.CompletedTask;
        }
    }
}
