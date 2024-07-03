using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Font = Microsoft.Maui.Graphics.Font;

namespace MauiGraphicsTest;

public class GraphicsDrawStatusEventArgs : EventArgs
{
    public bool IsStarted { get; set; }    // true: drawing started; false: drawing ended
}

public class GraphicsDrawable : IDrawable
{
    public const string FONT_NAME = "ArialMT";      // If "Arial", get many "CoreText performance note" warnings saying got font with PostScript name "ArialMT",
                                                    // and suggested only use PostScript names.
    public static Font FONT = new Font(FONT_NAME);
    public static Color FONT_COLOR = Colors.Black;
    public const int FONT_SIZE = 14;
    public const bool ANTI_ALIAS = false;
    public const int STROKE_SIZE = 1;

    public event EventHandler<GraphicsDrawStatusEventArgs>? GraphicsDrawStatus = null; // Event announcing status (started / ended) of drawing graphic items.


    public async void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (ViewUtils.ViewPixelsPerInch == ViewUtils.VIEW_PIXELS_PER_INCH_UNSET || ViewUtils.ViewScaleFeet == ViewUtils.VIEW_SCALE_FEET_UNSET)
        {
            return;
        }

        try
        {
            OnGraphicsDrawStarted();

            canvas.Font = new Font(FONT_NAME);
            canvas.FontColor = FONT_COLOR;
            canvas.FontSize = FONT_SIZE;
            canvas.Antialias = ANTI_ALIAS;
            canvas.StrokeSize = STROKE_SIZE;

            DrawItems(canvas);
        }
        catch (Exception ex)
        {
            await ShellUtils.ShowError($"Error drawing items: {ex.Message}");
        }
        finally
        {
            OnGraphicsDrawEnded();
        }
    }

    private void OnGraphicsDrawStarted()
    {
        GraphicsDrawStatus?.Invoke(this, new GraphicsDrawStatusEventArgs() { IsStarted = true });
    }

    private void OnGraphicsDrawEnded()
    {
        GraphicsDrawStatus?.Invoke(this, new GraphicsDrawStatusEventArgs() { IsStarted = false });
    }

    private void DrawItems(ICanvas canvas)
    {
        List<GraphicItem>? items = App.CurrentApp?.Items;

        if (items != null)
        {
            foreach (GraphicItem item in items)
            {
                item.Draw(canvas);
            }
        }
    }
}


public static class ViewUtils
{
    public const int VIEW_SITE_UNITS_PER_FOOT_INT = 120;
    public const double VIEW_SITE_UNITS_PER_FOOT_DBL = 120.0;

    public const int VIEW_PIXELS_PER_INCH_UNSET = 0;
    public const int VIEW_SCALE_FEET_UNSET = 0;
    public const int VIEW_SCALE_FEET_DEFAULT = 14;

    private static int _viewPixelsPerInch = VIEW_PIXELS_PER_INCH_UNSET;
    private static double _viewDensity = 1.0;
    private static double _viewScaleFeet = VIEW_SCALE_FEET_UNSET;
    private static int _viewScaleFeetMin = 1;
    private static int _viewScaleFeetMax = 35;

    public static double ViewDensity
    {
        get => _viewDensity;

        set
        {
            _viewDensity = value;
            UpdateViewScale();
        }
    }

    public static int ViewPixelsPerInch
    {
        get => _viewPixelsPerInch;

        set
        {
            _viewPixelsPerInch = value;
            UpdateViewScale();
        }
    }

    public static double ViewScaleFeet     // Site feet per display inch
    {
        get => _viewScaleFeet;

        set
        {
            _viewScaleFeet = value;
            UpdateViewScale();
        }

    }

    public static double ViewScale { get; set; }  // pixels / site unit (.1 inch)

    public static int ViewScaleFeetMin
    {
        get => _viewScaleFeetMin;
        set => _viewScaleFeetMin = value;
    }

    public static int ViewScaleFeetMax
    {
        get => _viewScaleFeetMax;
        set => _viewScaleFeetMax = value;
    }

    //
    // Conversion from inches or feet to site units and vice-versa
    //
    public static long InchesToSite(double nInches)
    {
        return Convert.ToInt32(nInches * 10.0);
    }

    public static long InchesToSite(long nInches)
    {
        return (nInches * 10);
    }

    public static double SiteToInches(long nSiteValue)
    {
        return ((double)nSiteValue / (double)10.0);
    }

    public static long FeetToSite(double nFeet)
    {
        return Convert.ToInt32(nFeet * VIEW_SITE_UNITS_PER_FOOT_DBL);
    }
    public static long FeetToSite(long nFeet)
    {
        return (nFeet * VIEW_SITE_UNITS_PER_FOOT_INT);
    }
    public static double SiteToFeet(long nSiteValue)
    {
        return ((double)nSiteValue / VIEW_SITE_UNITS_PER_FOOT_DBL);
    }

    //
    // Conversion from site units or feet to pixels and vice-versa
    //
    public static int SiteToPixels(long nSiteValue)
    {
        double nPix = (double)nSiteValue * ViewScale;
        return Convert.ToInt32(nPix);
    }

    public static long PixelsToSite(int nPixelValue)
    {
        double nSite = (double)nPixelValue / ViewScale;
        return Convert.ToInt32(nSite);
    }

    public static int SiteFeetToPixels(long nSiteFeetValue)
    {
        double nPix = ((double)(nSiteFeetValue * VIEW_SITE_UNITS_PER_FOOT_INT) * ViewScale);
        return Convert.ToInt32(nPix);
    }

    public static long PixelsToSiteFeet(int nPixelValue, bool bRoundUp = true)
    {
        double nFeet = (double)nPixelValue / (ViewScale * VIEW_SITE_UNITS_PER_FOOT_DBL);
        return (Convert.ToInt32(bRoundUp ? Math.Ceiling(nFeet) : nFeet));
    }

    public static void UpdateViewScale()
    {
        if (_viewScaleFeet != VIEW_SCALE_FEET_UNSET && _viewPixelsPerInch != VIEW_PIXELS_PER_INCH_UNSET)
        {
            ViewScale = (_viewPixelsPerInch /
                (VIEW_SITE_UNITS_PER_FOOT_DBL * _viewScaleFeet)) / _viewDensity;
        }
    }
}

public class GraphicItem
{
    public string ItemName { get; set; }
    public int ItemPos1_X { get; set; }
    public int ItemPos1_Y { get; set; }
    public int ItemPos2_X { get; set; }
    public int ItemPos2_Y { get; set; }

    public GraphicItem(string itemName, int itemPos1_X, int itemPos1_Y, int itemLength, int itemWidth)
    {
        ItemName = itemName;
        ItemPos1_X = (int)ViewUtils.FeetToSite(itemPos1_X);
        ItemPos1_Y = (int)ViewUtils.FeetToSite(itemPos1_Y);
        ItemPos2_X = (int)ViewUtils.FeetToSite(itemPos1_X + itemLength);
        ItemPos2_Y = (int)ViewUtils.FeetToSite(itemPos1_Y + itemWidth);
    }

    public void Draw(ICanvas canvas)
    {
        RectF rectItem = new()
        {
            Left = ViewUtils.SiteToPixels(ItemPos1_X),
            Top = ViewUtils.SiteToPixels(ItemPos1_Y),
            Right = ViewUtils.SiteToPixels(ItemPos2_X),
            Bottom = ViewUtils.SiteToPixels(ItemPos2_Y)
        };
        //Console.WriteLine($"Item: {ItemName}, Width: {rectItem.Width}, Height: {rectItem.Height}.");

        // Outline
        canvas.DrawCircle(rectItem.Center, rectItem.Width / 2);

        // Text
        SizeF size = canvas.GetStringSize(ItemName, GraphicsDrawable.FONT, GraphicsDrawable.FONT_SIZE);
        float inflateWidth = 0;
        float inflateHeight = 0;

        if (size.Width > rectItem.Width)
        {
            inflateWidth = (size.Width - rectItem.Width) / 2;
        }
        if (size.Height > rectItem.Height)
        {
            inflateHeight = (size.Height - rectItem.Height) / 2;
        }

        RectF rectTemp = rectItem;

        if (inflateWidth > 0 || inflateHeight > 0)
        {
            rectTemp = rectItem.Inflate(inflateWidth, inflateHeight);
        }

        canvas.DrawString(ItemName, rectTemp.Left, rectTemp.Top, rectTemp.Width, rectTemp.Height, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}


public class MainPageViewModel
{
    private bool _firstOnAppearing = true;

    private GraphicsView _itemsGraphicsView;

    public MainPageViewModel(GraphicsView itemsGraphicsView)
    {
        _itemsGraphicsView = itemsGraphicsView;

        ((GraphicsDrawable)_itemsGraphicsView.Drawable).GraphicsDrawStatus += OnGraphicsDrawStatus;
    }

    public async void OnAppearing()
    {
        try
        {
            await OnAppearingAsync();
        }
        catch (Exception ex)
        {
            await ShellUtils.ShowError($"Error while Graphics Page is appearing: {ex.Message}");
        }
    }

    private async Task OnAppearingAsync()
    {
        if (_firstOnAppearing)
        {
            await SetupViewSettings();

            _firstOnAppearing = false;
        }

        SetGraphicsViewSize();
    }

    private async Task SetupViewSettings()
    {
        ViewUtils.ViewDensity = DeviceDisplay.Current.MainDisplayInfo.Density;

        ViewUtils.ViewPixelsPerInch = await PromptPPI();

        ViewUtils.ViewScaleFeet = ViewUtils.VIEW_SCALE_FEET_DEFAULT;
    }

    private static async Task<int> PromptPPI()
    {
        string label = "Display PPI";

        int? displayPPI = await ShellUtils.PromptNumeric<int>(label, "Enter display pixels per inch:", string.Empty,
            s => AppUtils.ParseNumeric<int>(s, null), isRequired: true);

        if (!displayPPI.HasValue)
        {
            throw new ApplicationException($"No {label} value was supplied.");
        }

        return displayPPI.Value;
    }

    // SetGraphicsViewSize()
    //
    // This sets the desired GraphicsView size, and returns whether the size changed.
    //
    private bool SetGraphicsViewSize(double? newScaleFt = null/*TODO:, bool startingZoom = false*/)
    {
        bool sizeChanged = false;

        if (newScaleFt.HasValue)
        {
            ViewUtils.ViewScaleFeet = newScaleFt.Value;
        }

        double desiredWid = ViewUtils.SiteFeetToPixels(App.SITE_LENGTH_FT);
        double desiredHt = ViewUtils.SiteFeetToPixels(App.SITE_WIDTH_FT);
#if DEBUG
        Console.WriteLine($"SetGraphicsViewSize: Scale: '{newScaleFt}'. Desired Canvas Width: {desiredWid}, Canvas Height: {desiredHt}.");
#endif
        if (_itemsGraphicsView.WidthRequest != desiredWid || _itemsGraphicsView.HeightRequest != desiredHt)
        {
#if DEBUG
            Console.WriteLine($"SetGraphicsViewSize: Size changed.");
#endif
            sizeChanged = true;

            _itemsGraphicsView.WidthRequest = desiredWid;
            _itemsGraphicsView.HeightRequest = desiredHt;
        }

        return sizeChanged;
    }

    private void OnGraphicsDrawStatus(object? sender, GraphicsDrawStatusEventArgs e)
    {
        if (e.IsStarted)
        {
#if DEBUG
            Console.WriteLine("OnGraphicsDrawStatus: Drawing started.");
#endif
        }
        else
        {
#if DEBUG
            Console.WriteLine("OnGraphicsDrawStatus: Drawing ended.");
#endif
        }
    }

    public async void OnGraphicsTapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool isScaleValid = false;
            double? scaleValue = null;
            string initialScale = ViewUtils.ViewScaleFeet.ToString();

            do
            {
                scaleValue = await ShellUtils.PromptNumeric<double>("Display Scale", "Enter display scale (feet / display inch):", initialValueStr: initialScale,
                    s => AppUtils.ParseNumeric<double>(s, null));

                if (!scaleValue.HasValue)
                {
                    // User canceled
                    break;
                }

                isScaleValid = (scaleValue.Value >= ViewUtils.ViewScaleFeetMin && scaleValue.Value <= ViewUtils.ViewScaleFeetMax);

                if (!isScaleValid)
                {
                    await ShellUtils.ShowError($"Display Scale '{scaleValue.Value}' is invalid:\n" +
                        $"Display Scale must be from {ViewUtils.ViewScaleFeetMin} to {ViewUtils.ViewScaleFeetMax}.");

                    initialScale = scaleValue.Value.ToString();
                }
            } while (!isScaleValid);

            if (isScaleValid)
            {
                SetGraphicsViewSize(scaleValue);
            }
        }
        catch (Exception ex)
        {
            await ShellUtils.ShowError($"Error prompting for display scale value: {ex.Message}");
        }
    }
}
