
using System.Diagnostics;

namespace MauiGraphicsTest;


public partial class MainPage : ContentPage
{
    MainPageViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();

        BindingContext = _viewModel = new MainPageViewModel(ItemsGraphicsView);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.OnAppearing();
    }

    private void OnGraphicsTapped(object sender, TappedEventArgs e)
    {
        _viewModel.OnGraphicsTapped(sender, e);
    }
}
