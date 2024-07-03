namespace MauiGraphicsTest;


public partial class App : Application
{
    public const int MAX_ITEMS = 1000;

    public const int SITE_LENGTH_FT = 300;
    public const int SITE_WIDTH_FT = 420;

    public const int ITEM_MARGIN_FT = 6;

    public const int ITEM_SIZE_FT = 10; // Circular item - width and height the same


    public static App? CurrentApp => (App?)Current;

    public List<GraphicItem>? Items { get; set; }


    public App()
    {
        InitializeComponent();

        LoadItems();

        MainPage = new AppShell();
    }

    private void LoadItems()
    {
        Items = [];

        int itemNumber = 1;
        int x = ITEM_MARGIN_FT, y = ITEM_MARGIN_FT;

        do
        {
            Items.Add(new GraphicItem($"Item {itemNumber}", x, y, ITEM_SIZE_FT, ITEM_SIZE_FT));
            itemNumber++;

            x += ITEM_SIZE_FT + ITEM_MARGIN_FT;

            if ((x + ITEM_SIZE_FT) > SITE_LENGTH_FT)
            {
                x = ITEM_MARGIN_FT;
                y += ITEM_SIZE_FT + ITEM_MARGIN_FT;

                if ((y + ITEM_SIZE_FT) > SITE_WIDTH_FT)
                {
                    break;
                }
            }
        } while (itemNumber <= MAX_ITEMS);
    }
}

public static class AppUtils
{
    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static T ParseNumeric<T>(string valueStr, IFormatProvider? provider) where T : IParsable<T>
    {
        T returnVal;

        try
        {
            returnVal = T.Parse(valueStr, provider);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Value '{valueStr}' is invalid: {ex.Message}");
        }

        return returnVal;
    }

}
