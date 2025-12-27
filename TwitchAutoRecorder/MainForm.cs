using LibVLCSharp.Shared;
using System.Diagnostics;
using System.Text;
using TwitchAutoRecorder.Properties;

namespace TwitchAutoRecorder;

public partial class MainForm : Form
{
    private readonly System.Windows.Forms.Timer liveTimer = new() { Interval = 1000 };

    private CancellationTokenSource? _cts;
    private Task? _monitorTask;

    private TwitchApi? _twitch;
    private Recorder? _recorder;

    private string _ffmpegExe = "";
    private string _streamlinkExe = "streamlink";

    private readonly System.Windows.Forms.Timer _configSaveTimer = new() { Interval = 750 };

    private DateTimeOffset? _currentStreamStartedAtUtc;
    private bool _isCurrentlyLive;
    private StreamSnapshot? _lastLiveSnapshot;

    private readonly SemaphoreSlim _manualSplitLock = new(1, 1);
    private volatile bool _manualSplitInProgress = false;
    private volatile bool _hasRecordedThisLiveSession = false;

    private volatile bool _stopping = false;

    private LibVLC? _libVlc;
    private MediaPlayer? _mp;

    private readonly System.Windows.Forms.Timer _cutUiTimer = new() { Interval = 100 };
    private bool _timelineDragging;
    private long _cutDurationMs;

    private string? _cutInputPath;
    private long? _markInMs;
    private long? _markOutMs;
    private Media? _media;

    private sealed record Segment(long StartMs, long EndMs);
    private readonly List<Segment> _segmentsList = new();

    public MainForm()
    {
        InitializeComponent();

        _configSaveTimer.Tick += (_, __) =>
        {
            _configSaveTimer.Stop();
        };

        btnBrowse.Click += (_, __) => BrowseFolder();
        btnStart.Click += async (_, __) => await StartAsync();
        btnStop.Click += async (_, __) => await StopAsync();

        chkSplitOnGameChange.CheckedChanged += (_, __) =>
        {
            nudDebouncePolls.Enabled = chkSplitOnGameChange.Checked;
        };


        nudDebouncePolls.Enabled = chkSplitOnGameChange.Checked;

        liveTimer.Tick += (_, __) => UpdateLiveForLabel();
        liveTimer.Start();

        this.FormClosing += async (_, __) =>
        {
            try { await StopAsync(); } catch { }
            DisposeCutter();
        };

        Core.Initialize();
        _libVlc = new LibVLC();

        LoadConfig();
        DetectTools();
        InitCutter();
    }
    private void DetectTools()
    {
        var baseDir = AppContext.BaseDirectory;
        _ffmpegExe = Path.Combine(baseDir, "Tools", "ffmpeg", "ffmpeg.exe");
        _streamlinkExe = "streamlink";

        var okF = File.Exists(_ffmpegExe);
        var okS = IsOnPath("streamlink");

        lblTools.Text = $"Tools: ffmpeg={(okF ? "OK" : "MISSING")} | streamlink={(okS ? "OK (PATH)" : "MISSING (PATH)")}";
    }

    private static bool IsOnPath(string exeName)
    {
        try
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = exeName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            p.WaitForExit(3000);
            return p.ExitCode == 0;
        }
        catch { return false; }
    }
    private void BrowseFolder()
    {
        using var dlg = new FolderBrowserDialog { SelectedPath = txtOutputDir.Text };
        if (dlg.ShowDialog() == DialogResult.OK)
            txtOutputDir.Text = dlg.SelectedPath;
        SaveConfig();
    }

    private async Task StartAsync()
    {
        if (_cts != null) return;

        DetectTools();
        SaveConfig();

        if (!File.Exists(_ffmpegExe))
        {
            MessageBox.Show(
                "Missing bundled ffmpeg.\n\n" +
                $"Place ffmpeg.exe at: {_ffmpegExe}",
                "Tools missing"
            );
            return;
        }

        if (!IsOnPath("streamlink"))
        {
            MessageBox.Show(
                "Streamlink was not found in PATH.\n\n" +
                "Install Streamlink so 'streamlink --version' works in Command Prompt,\n" +
                "then restart this app.",
                "Tools missing"
            );
            return;
        }

        var streamer = txtStreamer.Text.Trim();
        if (string.IsNullOrWhiteSpace(streamer))
        {
            MessageBox.Show("Enter streamer login (user_login).");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtClientId.Text) || string.IsNullOrWhiteSpace(txtClientSecret.Text))
        {
            MessageBox.Show("Enter Twitch Client ID and Client Secret.");
            return;
        }

        Directory.CreateDirectory(txtOutputDir.Text);

        _cts = new CancellationTokenSource();
        _twitch = new TwitchApi(txtClientId.Text.Trim(), txtClientSecret.Text.Trim());
        _recorder = new Recorder(_streamlinkExe, _ffmpegExe);
        _recorder.Log += AppendLog;

        _hasRecordedThisLiveSession = false;
        _manualSplitInProgress = false;
        _stopping = false;

        btnStart.Enabled = false;
        btnStop.Enabled = true;

        SetStatus("Monitoring...");
        AppendLog("Started monitor loop.");

        _monitorTask = Task.Run(async () =>
        {
            try
            {
                await MonitorLoopAsync(streamer, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                AppendLog("Monitor loop cancelled.");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR: " + ex);
                BeginInvoke(new Action(() => MessageBox.Show(ex.Message, "Error")));
            }
            finally
            {
                BeginInvoke(new Action(() =>
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;

                    _recorder?.Dispose();
                    _twitch?.Dispose();
                    _recorder = null;
                    _twitch = null;

                    _cts?.Dispose();
                    _cts = null;

                    _isCurrentlyLive = false;
                    _currentStreamStartedAtUtc = null;
                    _lastLiveSnapshot = null;

                    _manualSplitInProgress = false;
                    _hasRecordedThisLiveSession = false;
                    _stopping = false;

                    SetStatus("Idle");
                    SetLiveFor("-");
                    SetGame("-");
                    SetFile("-");
                }));
            }
        });
    }

    private async Task StopAsync()
    {
        if (_cts == null) return;
        SaveConfig();
        _stopping = true;
        SetStatus("Stopping...");
        AppendLog("Stopping...");

        await _manualSplitLock.WaitAsync();
        _manualSplitLock.Release();

        _cts.Cancel();

        if (_recorder != null)
        {
            try { await _recorder.StopAsync(CancellationToken.None); } catch { }
        }

        if (_monitorTask != null)
        {
            try { await _monitorTask; } catch { }
            _monitorTask = null;
        }

        _stopping = false;

        BeginInvoke(new Action(() =>
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            SetStatus("Idle");
            SetLiveFor("-");
            SetGame("-");
            SetFile("-");
        }));
    }

    private async Task MonitorLoopAsync(string streamer, CancellationToken ct)
    {
        if (_twitch == null || _recorder == null)
            throw new InvalidOperationException("Not initialized.");

        var poll = TimeSpan.FromSeconds((double)nudPollSeconds.Value);
        var delay = TimeSpan.FromMinutes((double)nudDelayMinutes.Value);
        var debounceNeeded = (int)nudDebouncePolls.Value;

        string? currentGameId = null;
        string? pendingGameId = null;
        int pendingGameCount = 0;

        while (!ct.IsCancellationRequested)
        {
            var snap = await _twitch.GetStreamAsync(streamer, ct);

            _lastLiveSnapshot = snap.IsLive ? snap : null;
            _isCurrentlyLive = snap.IsLive;
            _currentStreamStartedAtUtc = snap.IsLive ? snap.StartedAtUtc : null;

            UpdateStatusFromState(snap);

            AppendLog($"Poll: live={snap.IsLive}, game='{snap.GameName}', id={snap.GameId}");

            if (_manualSplitInProgress)
            {
                await Task.Delay(250, ct);
                continue;
            }

            if (!snap.IsLive)
            {
                _hasRecordedThisLiveSession = false;

                if (_recorder.HasPendingFinalize)
                {
                    AppendLog("Stream is offline. Finalizing pending segment...");
                    await _recorder.FinalizeIfNeededAsync(CancellationToken.None);
                    SetFile("-");
                }

                if (_recorder.IsRecording)
                {
                    AppendLog("Stream went offline. Stopping recording...");
                    await _recorder.StopAsync(CancellationToken.None);
                    SetFile("-");
                }

                SetGame("-");
                await Task.Delay(poll, ct);
                continue;
            }

            SetGame($"{snap.GameName} ({snap.GameId})");

            if (!_recorder.IsRecording)
            {
                var wait = _hasRecordedThisLiveSession ? TimeSpan.Zero : delay;

                if (wait > TimeSpan.Zero)
                {
                    AppendLog($"Live. Waiting {wait.TotalMinutes:0} min before recording...");
                    await Task.Delay(wait, ct);
                }

                var snap2 = await _twitch.GetStreamAsync(streamer, ct);

                _lastLiveSnapshot = snap2.IsLive ? snap2 : null;
                _isCurrentlyLive = snap2.IsLive;
                _currentStreamStartedAtUtc = snap2.IsLive ? snap2.StartedAtUtc : null;

                UpdateStatusFromState(snap2);

                if (!snap2.IsLive)
                {
                    AppendLog("Went offline during delay; not starting.");
                    await Task.Delay(poll, ct);
                    continue;
                }

                currentGameId = snap2.GameId;
                pendingGameId = null;
                pendingGameCount = 0;

                var twitchUrl = $"https://twitch.tv/{streamer}";
                var rawPath = BuildSegmentPath(txtOutputDir.Text, snap2);

                SetFile(Path.GetFileName(Path.ChangeExtension(rawPath, ".mp4")));
                AppendLog($"Starting recording: {rawPath}");

                await _recorder.StartAsync(twitchUrl, rawPath, ct);

                _hasRecordedThisLiveSession = true;

                await Task.Delay(poll, ct);
                continue;
            }

            currentGameId ??= snap.GameId;

            bool splitEnabled = chkSplitOnGameChange.Checked;
            if (!splitEnabled)
            {
                pendingGameId = null;
                pendingGameCount = 0;
                await Task.Delay(poll, ct);
                continue;
            }

            if (snap.GameId != currentGameId)
            {
                if (pendingGameId == snap.GameId) pendingGameCount++;
                else { pendingGameId = snap.GameId; pendingGameCount = 1; }

                AppendLog($"Game change candidate: {snap.GameName} (count {pendingGameCount}/{debounceNeeded})");

                if (pendingGameCount >= debounceNeeded)
                {
                    currentGameId = snap.GameId;
                    pendingGameId = null;
                    pendingGameCount = 0;

                    var twitchUrl = $"https://twitch.tv/{streamer}";
                    var newRawPath = BuildSegmentPath(txtOutputDir.Text, snap);

                    AppendLog($"Splitting to new segment (game change): {newRawPath}");

                    await _recorder.RestartAsync(twitchUrl, newRawPath, CancellationToken.None);

                    SetFile(Path.GetFileName(Path.ChangeExtension(newRawPath, ".mp4")));
                }
            }
            else
            {
                pendingGameId = null;
                pendingGameCount = 0;
            }

            await Task.Delay(poll, ct);
        }
    }

    private void UpdateStatusFromState(StreamSnapshot? snap)
    {
        if (_stopping)
        {
            SetStatus("Stopping...");
            return;
        }

        if (_manualSplitInProgress)
        {
            SetStatus("Splitting (manual)...");
            return;
        }

        if (snap != null && !snap.IsLive)
        {
            SetStatus("Offline (monitoring)");
            return;
        }

        if (_recorder != null && _recorder.IsRecording)
        {
            SetStatus("Recording");
            return;
        }

        if (snap != null && snap.IsLive)
        {
            SetStatus("Live detected");
            return;
        }

        SetStatus("Monitoring...");
    }

    private string BuildSegmentPath(string outputDir, StreamSnapshot snap)
    {
        var streamer = snap.UserLogin;
        var folder = Path.Combine(outputDir, streamer);
        Directory.CreateDirectory(folder);

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var time = DateTime.Now.ToString("HH-mm-ss");

        var game = snap.GameName;
        var title = snap.Title;
        var streamId = snap.StreamId;

        var template = txtFileTemplate.Text.Trim();
        if (string.IsNullOrWhiteSpace(template))
            template = "{streamer}_{date}_{time}_{game}";

        var name = template
            .Replace("{streamer}", streamer, StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", date, StringComparison.OrdinalIgnoreCase)
            .Replace("{time}", time, StringComparison.OrdinalIgnoreCase)
            .Replace("{game}", game, StringComparison.OrdinalIgnoreCase)
            .Replace("{title}", title, StringComparison.OrdinalIgnoreCase)
            .Replace("{streamId}", streamId, StringComparison.OrdinalIgnoreCase);

        name = SanitizeFileName(name);
        name = TrimToMaxLength(name, 140);

        return Path.Combine(folder, $"{name}.ts");
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Recording";

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var ch in name)
            sb.Append(invalid.Contains(ch) ? '_' : ch);

        var s = sb.ToString().Trim();

        while (s.Contains("  ")) s = s.Replace("  ", " ");
        s = s.Trim('.', ' ');

        return string.IsNullOrWhiteSpace(s) ? "Recording" : s;
    }

    private static string TrimToMaxLength(string s, int max)
        => s.Length <= max ? s : s[..max].TrimEnd('_', ' ', '.');

    private static int ClampToRange(int value, int min, int max)
        => Math.Min(Math.Max(value, min), max);

    private void UpdateLiveForLabel()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(UpdateLiveForLabel));
            return;
        }

        if (!_isCurrentlyLive || _currentStreamStartedAtUtc == null || _currentStreamStartedAtUtc == DateTimeOffset.MinValue)
        {
            lblLiveFor.Text = "Live for: -";
            return;
        }

        var elapsed = DateTimeOffset.UtcNow - _currentStreamStartedAtUtc.Value;
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;

        var hours = (int)elapsed.TotalHours;
        var text = $"{hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        lblLiveFor.Text = $"Live for: {text}";
    }
    private void SetStatus(string s)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => SetStatus(s))); return; }
        lblStatus.Text = "Status: " + s;
    }

    private void SetLiveFor(string s)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => SetLiveFor(s))); return; }
        lblLiveFor.Text = "Live for: " + s;
    }

    private void SetGame(string s)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => SetGame(s))); return; }
        lblGame.Text = "Game: " + s;
    }

    private void SetFile(string s)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => SetFile(s))); return; }
        lblFile.Text = "File: " + s;
    }

    private void AppendLog(string msg)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => AppendLog(msg))); return; }
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        txtLog.ScrollToCaret();
    }

    private void InitCutter()
    {
        if (_libVlc == null)
            return;

        _mp = new MediaPlayer(_libVlc);
        _videoView.MediaPlayer = _mp;

        _btnOpen.Click += (_, __) => CutOpenFile();
        _btnPlayPause.Click += (_, __) => CutPlayPause();

        _timeline.MouseDown += (_, __) => _timelineDragging = true;
        _timeline.MouseUp += (_, __) =>
        {
            _timelineDragging = false;
            CutSeekFromTimeline();
        };

        _btnMarkIn.Click += (_, __) => CutMarkIn();
        _btnMarkOut.Click += (_, __) => CutMarkOut();
        _btnAddSegment.Click += (_, __) => CutAddSegment();
        _btnRemoveSegment.Click += (_, __) => CutRemoveSelected();
        _btnExport.Click += async (_, __) => await CutExportAsync();

        _cutUiTimer.Tick += (_, __) => CutUpdateUiTick();
        _cutUiTimer.Start();
    }

    private void DisposeCutter()
    {
        try { _cutUiTimer.Stop(); } catch { }
        try { _mp?.Stop(); } catch { }

        _mp?.Dispose();
        _mp = null;

        _libVlc?.Dispose();
        _libVlc = null;
    }

    private void LoadVideo(string path)
    {
        if (_libVlc == null)
            throw new InvalidOperationException("LibVLC is not initialized.");

        UnloadVideo();

        _mp = new MediaPlayer(_libVlc);
        _videoView.MediaPlayer = _mp;

        _media = new Media(_libVlc, new Uri(path));
        _mp.Media = _media;

        _mp.Play();
        _btnPlayPause.Text = "Pause";
    }


    private void CutOpenFile()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*",
            Title = "Select an MP4 file"
        };

        if (ofd.ShowDialog() != DialogResult.OK)
            return;

        _cutInputPath = ofd.FileName;
        _segmentsList.Clear();
        _segments.Items.Clear();
        _markInMs = null;
        _markOutMs = null;
        _lblIn.Text = "Start cut: -";
        _lblOut.Text = "End cut: -";
        _lblTime.Text = "00:00:00.000 / 00:00:00.000";

        var outDefault = Path.Combine(
            Path.GetDirectoryName(_cutInputPath)!,
            Path.GetFileNameWithoutExtension(_cutInputPath) + "_cut.mp4"
        );
        _txtOutput.Text = outDefault;

        try
        {
            LoadVideo(_cutInputPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load video:\n" + ex.Message, "Error");
        }
    }

    private void UnloadVideo()
    {
        if (InvokeRequired)
        {
            BeginInvoke(UnloadVideo);
            return;
        }

        try
        {
            if (_mp != null)
            {
                try { _mp.Stop(); } catch { }
                _mp.Media = null;
            }

            _videoView.MediaPlayer = null;

            _media?.Dispose();
            _media = null;

            _mp?.Dispose();
            _mp = null;

            _timeline.Value = 0;
            _lblTime.Text = "00:00:00.000 / 00:00:00.000";
            _lblIn.Text = "Start cut: -";
            _lblOut.Text = "End cut: -";
            _segments.Items.Clear();

            AppendLog("Video unloaded (ready for new file).");
        }
        catch (Exception ex)
        {
            AppendLog("UnloadVideo failed: " + ex.Message);
        }
    }

    private void CutPlayPause()
    {
        if (_mp == null) return;

        if (_mp.IsPlaying)
        {
            _mp.Pause();
            _btnPlayPause.Text = "Play";
        }
        else
        {
            _mp.Play();
            _btnPlayPause.Text = "Pause";
        }
    }

    private void CutSeekFromTimeline()
    {
        if (_mp == null || _cutDurationMs <= 0) return;

        var ratio = _timeline.Value / (double)_timeline.Maximum;
        var target = (long)(_cutDurationMs * ratio);
        _mp.Time = target;
    }

    private void CutMarkIn()
    {
        if (_mp == null) return;
        _markInMs = _mp.Time;
        _lblIn.Text = "Start cut: " + FormatMs(_markInMs.Value);
    }

    private void CutMarkOut()
    {
        if (_mp == null) return;
        _markOutMs = _mp.Time;
        _lblOut.Text = "End cut: " + FormatMs(_markOutMs.Value);
    }

    private void CutAddSegment()
    {
        if (_cutInputPath == null)
        {
            MessageBox.Show("Open a file first.");
            return;
        }

        if (_markInMs == null || _markOutMs == null)
        {
            MessageBox.Show("Use 'Begin cut' and 'End cut' first.");
            return;
        }

        var a = _markInMs.Value;
        var b = _markOutMs.Value;
        if (b <= a)
        {
            MessageBox.Show("'End cut' must be after 'Begin cut'.");
            return;
        }

        var seg = new Segment(a, b);
        _segmentsList.Add(seg);

        var dur = seg.EndMs - seg.StartMs;
        var item = new ListViewItem(new[]
        {
        FormatMs(seg.StartMs),
        FormatMs(seg.EndMs),
        FormatMs(dur)
    });

        _segments.Items.Add(item);
    }

    private void CutRemoveSelected()
    {
        if (_segments.SelectedIndices.Count == 0) return;

        var idxs = _segments.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        foreach (var idx in idxs)
        {
            _segments.Items.RemoveAt(idx);
            _segmentsList.RemoveAt(idx);
        }
    }

    private async Task CutExportAsync()
    {
        if (_cutInputPath == null)
        {
            MessageBox.Show("Open a file first.");
            return;
        }

        if (!File.Exists(_ffmpegExe))
        {
            MessageBox.Show("FFmpeg not found:\n" + _ffmpegExe);
            return;
        }

        if (_segmentsList.Count == 0)
        {
            MessageBox.Show("Add at least one cut section first.");
            return;
        }

        var outputPath = _txtOutput.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            MessageBox.Show("Choose an output file name.");
            return;
        }

        try
        {
            tabControl1.Enabled = false;

            if (_cutDurationMs <= 0)
            {
                MessageBox.Show("Video duration not known yet. Press Play for a moment and try again.");
                return;
            }

            var cuts = _segmentsList
                .Select(s => (Start: Math.Max(0, s.StartMs), End: Math.Min(_cutDurationMs, s.EndMs)))
                .Where(s => s.End > s.Start)
                .OrderBy(s => s.Start)
                .ToList();

            var mergedCuts = new List<(long Start, long End)>();
            foreach (var c in cuts)
            {
                if (mergedCuts.Count == 0)
                {
                    mergedCuts.Add(c);
                    continue;
                }

                var last = mergedCuts[^1];
                if (c.Start <= last.End)
                    mergedCuts[^1] = (last.Start, Math.Max(last.End, c.End));
                else
                    mergedCuts.Add(c);
            }

            var keeps = new List<(long Start, long End)>();
            long cur = 0;

            foreach (var cut in mergedCuts)
            {
                if (cut.Start > cur)
                    keeps.Add((cur, cut.Start));
                cur = Math.Max(cur, cut.End);
            }

            if (cur < _cutDurationMs)
                keeps.Add((cur, _cutDurationMs));

            keeps = keeps.Where(k => k.End - k.Start > 200).ToList();

            if (keeps.Count == 0)
            {
                MessageBox.Show("All content was cut out. Nothing left to export.");
                return;
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "TwitchAutoRecorder_Cut", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var segFiles = new List<string>();
            for (int i = 0; i < keeps.Count; i++)
            {
                var (start, end) = keeps[i];
                var segPath = Path.Combine(tempDir, $"keep_{i:000}.ts");
                segFiles.Add(segPath);

                var args = $"-hide_banner -y -loglevel error " +
                           $"-ss {ToFfmpegTime(start)} -to {ToFfmpegTime(end)} " +
                           $"-i \"{_cutInputPath}\" " +
                           $"-c copy -f mpegts \"{segPath}\"";

                await RunFfmpegAsync(args);
            }

            var listPath = Path.Combine(tempDir, "concat.txt");
            var sb = new StringBuilder();
            foreach (var f in segFiles)
            {
                var full = Path.GetFullPath(f);
                sb.AppendLine($"file '{full.Replace("'", "'\\''")}'");
            }
            File.WriteAllText(listPath, sb.ToString(), new UTF8Encoding(false));

            var joinedTs = Path.Combine(tempDir, "joined.ts");

            var concatArgs = $"-hide_banner -y -loglevel error " +
                             $"-f concat -safe 0 -i \"{listPath}\" " +
                             $"-c copy \"{joinedTs}\"";

            await RunFfmpegAsync(concatArgs);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var remuxArgs = $"-hide_banner -y -loglevel error " +
                            $"-i \"{joinedTs}\" " +
                            $"-c copy -movflags +faststart \"{outputPath}\"";

            await RunFfmpegAsync(remuxArgs);

            MessageBox.Show("Export complete:\n" + outputPath, "Done");


            try { Directory.Delete(tempDir, true); } catch { }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Export failed:\n" + ex.Message, "Error");
        }
        finally
        {
            tabControl1.Enabled = true;
        }
    }

    private async Task RunFfmpegAsync(string args)
    {
        if (!File.Exists(_ffmpegExe))
            throw new FileNotFoundException("ffmpeg not found", _ffmpegExe);

        var psi = new ProcessStartInfo
        {
            FileName = _ffmpegExe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        };

        using var p = new Process { StartInfo = psi };

        if (!p.Start())
            throw new Exception("Failed to start ffmpeg.");

        var stdOutTask = p.StandardOutput.ReadToEndAsync();
        var stdErrTask = p.StandardError.ReadToEndAsync();

        await p.WaitForExitAsync();

        var stdout = await stdOutTask;
        var stderr = await stdErrTask;

        if (p.ExitCode != 0)
        {
            var msg = new StringBuilder();
            msg.AppendLine($"ffmpeg exited with code {p.ExitCode}");
            msg.AppendLine();
            msg.AppendLine("Args:");
            msg.AppendLine(args);
            msg.AppendLine();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                msg.AppendLine("stderr:");
                msg.AppendLine(stderr.Trim());
            }
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                msg.AppendLine();
                msg.AppendLine("stdout:");
                msg.AppendLine(stdout.Trim());
            }
            throw new Exception(msg.ToString());
        }
    }

    private void CutUpdateUiTick()
    {
        if (_mp == null) return;

        var dur = _mp.Length;
        if (dur > 0) _cutDurationMs = dur;

        var cur = _mp.Time;

        if (!_timelineDragging && _cutDurationMs > 0)
        {
            var ratio = Math.Clamp(cur / (double)_cutDurationMs, 0, 1);
            _timeline.Value = (int)Math.Round(ratio * _timeline.Maximum);
        }

        _lblTime.Text = $"{FormatMs(cur)} / {FormatMs(_cutDurationMs)}";
    }

    private static string FormatMs(long ms)
    {
        if (ms < 0) ms = 0;
        var ts = TimeSpan.FromMilliseconds(ms);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }

    private static string ToFfmpegTime(long ms)
    {
        if (ms < 0) ms = 0;
        var ts = TimeSpan.FromMilliseconds(ms);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }

    private void SaveConfig()
    {
        Settings.Default.Streamer = txtStreamer.Text;
        Settings.Default.TwitchClientID = txtClientId.Text;
        Settings.Default.ClientSecret = txtClientSecret.Text;
        Settings.Default.OutputFolder = txtOutputDir.Text;
        Settings.Default.FilenameTemplate = txtFileTemplate.Text;
        Settings.Default.PollInterval = nudPollSeconds.Value;
        Settings.Default.DelayBeforeRecord = nudDelayMinutes.Value;
        Settings.Default.Debounce = nudDebouncePolls.Value;
        Settings.Default.SplitOnGameChange = chkSplitOnGameChange.Checked;
        Settings.Default.Save();
    }

    private void LoadConfig()
    {
        txtStreamer.Text = Settings.Default.Streamer;
        txtClientId.Text = Settings.Default.TwitchClientID;
        txtClientSecret.Text = Settings.Default.ClientSecret;
        txtOutputDir.Text = Settings.Default.OutputFolder;
        txtFileTemplate.Text = Settings.Default.FilenameTemplate;
        nudPollSeconds.Value = Settings.Default.PollInterval;
        nudDelayMinutes.Value = Settings.Default.DelayBeforeRecord;
        nudDebouncePolls.Value = Settings.Default.Debounce;
        chkSplitOnGameChange.Checked = Settings.Default.SplitOnGameChange;
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveConfig();
    }

    private void _btnUnload_Click(object sender, EventArgs e)
    {
        UnloadVideo();
    }
}
