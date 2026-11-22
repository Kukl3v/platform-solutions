using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Client.ViewModels;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class WholeNumberRangeTextAttribute(int minimum, int maximum) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is string text &&
               int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) &&
               number >= minimum && number <= maximum
            ? ValidationResult.Success
            : new ValidationResult($"Введите целое число от {minimum:N0} до {maximum:N0}.");
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class MatrixNumberTextAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || validationContext.ObjectInstance is not MainViewModel viewModel)
        {
            return new ValidationResult("Введите число.");
        }

        if (viewModel.UseFractions)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
                   double.IsFinite(number)
                ? ValidationResult.Success
                : new ValidationResult("Введите конечное число (разделитель — точка).");
        }

        return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
            ? ValidationResult.Success
            : new ValidationResult("Введите целое число.");
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class MaximumNotLessThanAttribute(string minimumPropertyName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string maximumText || validationContext.ObjectInstance is not MainViewModel viewModel)
        {
            return ValidationResult.Success;
        }

        var minimumText = validationContext.ObjectType
            .GetProperty(minimumPropertyName)?
            .GetValue(validationContext.ObjectInstance) as string;

        if (viewModel.UseFractions)
        {
            if (!double.TryParse(minimumText, NumberStyles.Float, CultureInfo.InvariantCulture, out var minimum) ||
                !double.TryParse(maximumText, NumberStyles.Float, CultureInfo.InvariantCulture, out var maximum))
            {
                return ValidationResult.Success;
            }

            if (!double.IsFinite(maximum - minimum))
            {
                return new ValidationResult("Диапазон слишком велик.");
            }

            return maximum >= minimum
                ? ValidationResult.Success
                : new ValidationResult("Максимум должен быть не меньше минимума.");
        }

        if (!long.TryParse(minimumText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerMinimum) ||
            !long.TryParse(maximumText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerMaximum))
        {
            return ValidationResult.Success;
        }

        return integerMaximum >= integerMinimum
            ? ValidationResult.Success
            : new ValidationResult("Максимум должен быть не меньше минимума.");
    }
}

