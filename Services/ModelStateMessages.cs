using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Alumni_Management_System.Services;

public static class ModelStateMessages
{
    // A blank box for a non-nullable number or date can't be bound, so MVC
    // reports a generic error before [Required] ever runs - swap in the
    // field's own readable message (the one the browser already shows).
    public static void UseClearBlankMessage(this ModelStateDictionary modelState, string key, string message)
    {
        if (modelState.TryGetValue(key, out var entry)
            && entry.Errors.Count > 0
            && string.IsNullOrWhiteSpace(entry.AttemptedValue))
        {
            entry.Errors.Clear();
            modelState.AddModelError(key, message);
        }
    }
}
