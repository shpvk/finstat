using System.ComponentModel;
using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop.Views;

public partial class OverviewView : ContentView
{
    private OverviewViewModel? model;
    private bool wasReady;

    public OverviewView()
    {
        InitializeComponent();
        BindingContextChanged += (sender, args) => Attach();
    }

    private void Attach()
    {
        if (model is not null)
            model.PropertyChanged -= OnModelChanged;
        model = BindingContext as OverviewViewModel;
        if (model is null)
            return;
        wasReady = model.IsReady;
        model.PropertyChanged += OnModelChanged;
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(OverviewViewModel.IsReady) || model is null)
            return;
        if (model.IsReady && !wasReady)
            Donut.Reveal();
        wasReady = model.IsReady;
    }
}
