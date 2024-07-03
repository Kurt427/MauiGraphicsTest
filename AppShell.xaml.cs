
namespace MauiGraphicsTest;

public static class ShellUtils
{
    public const string GENERAL_ERR_TITLE = "Graphics Test";


    public static async Task ShowError(string err, string title = null)
    {
        await Shell.Current.CurrentPage.DisplayAlert((title is null) ? GENERAL_ERR_TITLE : title, err, "OK");
    }

    public static async Task<bool> ShowPrompt(string question, string acceptText, string cancelText, string title = null)
    {
        return await Shell.Current.CurrentPage.DisplayAlert((title is null) ? GENERAL_ERR_TITLE : title, question,
            acceptText, cancelText);
    }

    public static Dictionary<string, object> CreateNavParam(string paramName, object param)
    {
        Dictionary<string, object> navParam = new Dictionary<string, object>
    {
        { paramName, param }
    };

        return navParam;
    }

    public static async Task<T?> PromptNumeric<T>(string label, string prompt, string initialValueStr, Func<string,T> valueParser, bool isRequired = false) where T : struct
    {
        T? returnValue = null;
        string? err;

        do
        {
            err = null;

            string valueStr = await Shell.Current.CurrentPage.DisplayPromptAsync(label, prompt, keyboard: Keyboard.Numeric, initialValue: initialValueStr);

            if (valueStr == null)
            {
                // User canceled
                break;
            }
            else if (valueStr.IsEmpty() && isRequired)
            {
                err = $"{label} value is required.";
                await ShellUtils.ShowError(err);
                continue;
            }

            try
            {
                T newVal = valueParser(valueStr);

                returnValue = newVal;
            }
            catch (Exception)
            {
                err = $"{label} value '{valueStr}' is not a valid number.";
            }

            if (err != null)
            {
                if (typeof(T) == typeof(int) && valueStr.Contains('.'))
                {
                    err += $"\n(Value must be a whole number (no \".\"))";
                }

                await ShellUtils.ShowError(err);
            }
        } while (err != null);

        return returnValue;
    }
}

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }
}
