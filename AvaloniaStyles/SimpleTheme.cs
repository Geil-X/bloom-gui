namespace AvaloniaStyles;

using Avalonia;
using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

public enum SimpleThemeMode
{
    Light,
    Dark
}

public class SimpleTheme : AvaloniaObject, IStyle, IResourceProvider
{
    public static readonly StyledProperty<SimpleThemeMode> ModeProperty =
        AvaloniaProperty.Register<SimpleTheme, SimpleThemeMode>(nameof(Mode));

    private bool IsLoading;
    private IStyle? _loaded;
    private Styles SharedStyles = new();
    private Styles SimpleDark = new();
    private Styles SimpleLight = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTheme"/> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    public SimpleTheme(Uri baseUri)
    {
        InitStyles(baseUri);
        Mode = SimpleThemeMode.Dark;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTheme"/> class.
    /// </summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    public SimpleTheme(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetService(typeof(IUriContext));
        if (service == null)
        {
            throw new Exception("There is no service object of type IUriContext!");
        }

        var baseUri = ((IUriContext) service).BaseUri;
        InitStyles(baseUri);
    }

    public event EventHandler OwnerChanged
    {
        add
        {
            if (Loaded is IResourceProvider rp)
            {
                rp.OwnerChanged += value;
            }
        }
        remove
        {
            if (Loaded is IResourceProvider rp)
            {
                rp.OwnerChanged -= value;
            }
        }
    }

    IReadOnlyList<IStyle> IStyle.Children => _loaded?.Children ?? Array.Empty<IStyle>();

    bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

    public IStyle Loaded
    {
        get
        {
            if (_loaded != null) return _loaded!;
            IsLoading = true;

            _loaded = Mode switch
            {
                SimpleThemeMode.Light => new Styles {SharedStyles, SimpleLight},
                SimpleThemeMode.Dark => new Styles {SharedStyles, SimpleDark},
                _ => _loaded
            };

            IsLoading = false;

            return _loaded!;
        }
    }

/// <summary>
    /// Gets or sets the mode of the fluent theme (light, dark).
    /// </summary>
    public SimpleThemeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

    void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);

    void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

    public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

    public bool TryGetResource(object key, out object? value)
    {
        if (!IsLoading && Loaded is IResourceProvider p)
        {
            return p.TryGetResource(key, out value);
        }

        value = null;
        return false;
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ModeProperty)
        {
            if (Mode == SimpleThemeMode.Dark)
            {
                (Loaded as Styles)![1] = SimpleDark[0];
            }
            else
            {
                (Loaded as Styles)![1] = SimpleLight[0];
            }
        }
    }

    private void InitStyles(Uri baseUri)
    {
        SharedStyles = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
            },
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml")
            }
        };
        SimpleLight = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml")
            }
        };

        SimpleDark = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml")
            }
        };
    }
}