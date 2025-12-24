using System.Diagnostics;

namespace TwitchAutoRecorder;

public sealed class Recorder : IDisposable
{
    private readonly string _streamlinkExe;
    private readonly string _ffmpegExe;

    private Process? _streamlink;

    private bool _finalizeRequested;

    public bool IsRecording => _streamlink != null && !_streamlink.HasExited;

    public string? CurrentRawPath { get; private set; }

    public string? CurrentMp4Path { get; private set; }

    public bool HasPendingFinalize
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CurrentRawPath) || string.IsNullOrWhiteSpace(CurrentMp4Path))
                return false;

            return File.Exists(CurrentRawPath) && !File.Exists(CurrentMp4Path);
        }
    }

    public event Action<string>? Log;

    public Recorder(string streamlinkExe, string ffmpegExe)
    {
        _streamlinkExe = streamlinkExe;
        _ffmpegExe = ffmpegExe;
    }

    public Task StartAsync(string twitchUrl, string rawPath, CancellationToken ct)
    {
        if (IsRecording) throw new InvalidOperationException("Already recording.");

        Directory.CreateDirectory(Path.GetDirectoryName(rawPath)!);

        CurrentRawPath = rawPath;
        CurrentMp4Path = Path.ChangeExtension(rawPath, ".mp4");
        _finalizeRequested = false;

        var args = $"\"{twitchUrl}\" best -o \"{rawPath}\" --twitch-disable-ads";

        _streamlink = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _streamlinkExe,
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _streamlink.Exited += (_, __) =>
        {
            try
            {
                Log?.Invoke($"streamlink exited with code {_streamlink.ExitCode}");
                _finalizeRequested = true;
            }
            catch { }
        };

        Log?.Invoke($"Starting streamlink: {_streamlinkExe} {args}");
        if (!_streamlink.Start())
            throw new InvalidOperationException("Failed to start streamlink.");

        _ = PumpErrorsAsync(_streamlink, "streamlink", ct);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        await StopStreamlinkIfRunningAsync();

        await FinalizeIfNeededAsync(ct);
    }

    public async Task FinalizeIfNeededAsync(CancellationToken ct)
    {
        if (!HasPendingFinalize)
            return;

        if (IsRecording)
            return;

        if (!_finalizeRequested)
        {

        }

        var raw = CurrentRawPath!;
        var mp4 = CurrentMp4Path!;

        Log?.Invoke($"Finalizing: remux {Path.GetFileName(raw)} -> {Path.GetFileName(mp4)}");
        await RemuxToMp4Async(raw, mp4, ct);

        if (File.Exists(mp4))
        {
            CurrentRawPath = null;
            CurrentMp4Path = null;
            _finalizeRequested = false;
        }
    }

    public async Task RestartAsync(string twitchUrl, string newRawPath, CancellationToken ct)
    {
        await StopAsync(CancellationToken.None);
        await StartAsync(twitchUrl, newRawPath, ct);
    }

    private async Task StopStreamlinkIfRunningAsync()
    {
        if (_streamlink == null) return;

        try
        {
            if (!_streamlink.HasExited)
            {
                Log?.Invoke("Stopping streamlink...");
                _streamlink.Kill(entireProcessTree: true);
            }
        }
        catch { }

        try { await _streamlink.WaitForExitAsync(); } catch { }

        _streamlink.Dispose();
        _streamlink = null;

        _finalizeRequested = true;
    }

    private async Task RemuxToMp4Async(string rawPath, string mp4Path, CancellationToken ct)
    {
        var args =
            $"-hide_banner -loglevel warning -y " +
            $"-i \"{rawPath}\" -c copy -movflags +faststart \"{mp4Path}\"";

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegExe,
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        p.Exited += (_, __) =>
        {
            try { Log?.Invoke($"ffmpeg remux exited with code {p.ExitCode}"); } catch { }
        };

        p.Start();
        _ = PumpErrorsAsync(p, "ffmpeg-remux", ct);

        await p.WaitForExitAsync(ct);

        if (p.ExitCode == 0 && File.Exists(mp4Path))
        {
            try { File.Delete(rawPath); } catch { }
        }
        else
        {
            Log?.Invoke($"Remux failed; keeping raw: {rawPath}");
        }

        p.Dispose();
    }

    private async Task PumpErrorsAsync(Process p, string tag, CancellationToken ct)
    {
        try
        {
            while (!p.StandardError.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await p.StandardError.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Log?.Invoke($"[{tag}] {line}");
            }
        }
        catch { }
    }

    public void Dispose()
    {
        try { _streamlink?.Kill(entireProcessTree: true); } catch { }
        _streamlink?.Dispose();
    }
}
