using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace LibVLCSharp.Avalonia.FuncUI;

public enum VideoState
{
    Play,
    Pause,
    Stop
}

/// <summary>
///     Avalonia VideoView for Windows, Linux and Mac.
/// </summary>
public class VideoView : NativeControlHost
{
    /// <summary>
    ///     Media Data Bound property
    /// </summary>
    /// <summary>
    ///     Defines the <see cref="Media" /> property.
    /// </summary>
    public static readonly DirectProperty<VideoView, Media?> MediaProperty =
        AvaloniaProperty.RegisterDirect<VideoView, Media?>(
            nameof(Media),
            o => o.Media,
            (o, v) => o.Media = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     VideoState Data Bound property
    /// </summary>
    /// <summary>
    ///     Defines the <see cref="VideoState" /> property.
    /// </summary>
    public static readonly DirectProperty<VideoView, VideoState> VideoStateProperty =
        AvaloniaProperty.RegisterDirect<VideoView, VideoState>(
            nameof(VideoState),
            o => o.VideoState,
            (o, v) => o.VideoState = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     MediaPlayer Data Bound property
    /// </summary>
    /// <summary>
    ///     Defines the <see cref="MediaPlayer" /> property.
    /// </summary>
    public static readonly DirectProperty<VideoView, MediaPlayer?> MediaPlayerProperty =
        AvaloniaProperty.RegisterDirect<VideoView, MediaPlayer?>(
            nameof(MediaPlayer),
            o => o.MediaPlayer,
            (o, v) => o.MediaPlayer = v,
            defaultBindingMode: BindingMode.TwoWay);

    private MediaPlayer? _mediaPlayer;
    private IPlatformHandle? _platformHandle;


    private VideoState _videoState = VideoState.Stop;

    public Media? Media
    {
        get => MediaPlayer?.Media;
        set
        {
            if (MediaPlayer is null) return;
            MediaPlayer.Media = value;
        }
    }

    public VideoState VideoState
    {
        get => _videoState;
        set
        {
            switch (value)
            {
                case VideoState.Play:
                    Play();
                    break;
                case VideoState.Pause:
                    Pause();
                    break;
                case VideoState.Stop:
                    Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the MediaPlayer that will be displayed.
    /// </summary>
    public MediaPlayer? MediaPlayer
    {
        get => _mediaPlayer;
        set
        {
            if (ReferenceEquals(_mediaPlayer, value)) return;

            Detach();
            _mediaPlayer = value;
            Attach();
        }
    }

    private void Play()
    {
        if (MediaPlayer is null || Media is null) return;

        if (MediaPlayer.Hwnd == IntPtr.Zero && MediaPlayer.XWindow == 0U &&
            MediaPlayer.NsObject == IntPtr.Zero) Attach();

        Console.WriteLine("Play");
        MediaPlayer?.Play(Media);
        _videoState = VideoState.Play;
    }

    private void Pause()
    {
        if (Media != null && MediaPlayer is {IsPlaying: true}) MediaPlayer?.Pause();

        Console.WriteLine("Pause");
        _videoState = VideoState.Pause;
    }

    private void Stop()
    {
        if (Media != null && MediaPlayer is {IsPlaying: true}) MediaPlayer?.Stop();

        Console.WriteLine("Stop");
        _videoState = VideoState.Stop;
    }

    private void Attach()
    {
        if (_mediaPlayer == null || _platformHandle == null || !IsInitialized)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _mediaPlayer.Hwnd = _platformHandle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _mediaPlayer.XWindow = (uint) _platformHandle.Handle;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) _mediaPlayer.NsObject = _platformHandle.Handle;
    }

    private void Detach()
    {
        if (_mediaPlayer == null)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _mediaPlayer.Hwnd = IntPtr.Zero;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _mediaPlayer.XWindow = 0;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) _mediaPlayer.NsObject = IntPtr.Zero;
    }

    /// <inheritdoc />
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _platformHandle = base.CreateNativeControlCore(parent);

        if (_mediaPlayer == null)
            return _platformHandle;

        Attach();

        return _platformHandle;
    }

    /// <inheritdoc />
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        Detach();
        base.DestroyNativeControlCore(control);
        _platformHandle = null;
    }
}