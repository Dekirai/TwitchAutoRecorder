namespace TwitchAutoRecorder
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            txtStreamer = new TextBox();
            btnBrowse = new Button();
            nudPollSeconds = new NumericUpDown();
            lblStatus = new Label();
            lblLiveFor = new Label();
            lblGame = new Label();
            lblFile = new Label();
            txtLog = new TextBox();
            btnStart = new Button();
            lblTokenHint = new Label();
            btnStop = new Button();
            txtClientId = new TextBox();
            txtClientSecret = new TextBox();
            txtOutputDir = new TextBox();
            nudDelayMinutes = new NumericUpDown();
            nudDebouncePolls = new NumericUpDown();
            chkSplitOnGameChange = new CheckBox();
            txtFileTemplate = new TextBox();
            lblTools = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            _segments = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            _videoView = new LibVLCSharp.WinForms.VideoView();
            _lblTime = new Label();
            _timeline = new TrackBar();
            _lblOut = new Label();
            _lblIn = new Label();
            _btnRemoveSegment = new Button();
            _btnAddSegment = new Button();
            _btnMarkOut = new Button();
            _btnMarkIn = new Button();
            _btnPlayPause = new Button();
            _btnOpen = new Button();
            _btnExport = new Button();
            _txtOutput = new TextBox();
            label9 = new Label();
            _btnUnload = new Button();
            ((System.ComponentModel.ISupportInitialize)nudPollSeconds).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDelayMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDebouncePolls).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_videoView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_timeline).BeginInit();
            SuspendLayout();
            // 
            // txtStreamer
            // 
            txtStreamer.Location = new Point(182, 9);
            txtStreamer.Name = "txtStreamer";
            txtStreamer.Size = new Size(240, 23);
            txtStreamer.TabIndex = 46;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(608, 96);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 24);
            btnBrowse.TabIndex = 33;
            btnBrowse.Text = "Browse...";
            // 
            // nudPollSeconds
            // 
            nudPollSeconds.Location = new Point(482, 125);
            nudPollSeconds.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            nudPollSeconds.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            nudPollSeconds.Name = "nudPollSeconds";
            nudPollSeconds.Size = new Size(120, 23);
            nudPollSeconds.TabIndex = 35;
            nudPollSeconds.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(182, 299);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(64, 15);
            lblStatus.TabIndex = 41;
            lblStatus.Text = "Status: Idle";
            // 
            // lblLiveFor
            // 
            lblLiveFor.AutoSize = true;
            lblLiveFor.Location = new Point(182, 319);
            lblLiveFor.Name = "lblLiveFor";
            lblLiveFor.Size = new Size(57, 15);
            lblLiveFor.TabIndex = 42;
            lblLiveFor.Text = "Live for: -";
            // 
            // lblGame
            // 
            lblGame.AutoSize = true;
            lblGame.Location = new Point(182, 339);
            lblGame.Name = "lblGame";
            lblGame.Size = new Size(49, 15);
            lblGame.TabIndex = 43;
            lblGame.Text = "Game: -";
            // 
            // lblFile
            // 
            lblFile.AutoSize = true;
            lblFile.Location = new Point(182, 359);
            lblFile.Name = "lblFile";
            lblFile.Size = new Size(36, 15);
            lblFile.TabIndex = 44;
            lblFile.Text = "File: -";
            // 
            // txtLog
            // 
            txtLog.Location = new Point(6, 377);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(947, 207);
            txtLog.TabIndex = 45;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(182, 273);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(120, 23);
            btnStart.TabIndex = 27;
            btnStart.Text = "Start";
            // 
            // lblTokenHint
            // 
            lblTokenHint.AutoSize = true;
            lblTokenHint.ForeColor = SystemColors.GrayText;
            lblTokenHint.Location = new Point(182, 234);
            lblTokenHint.Name = "lblTokenHint";
            lblTokenHint.Padding = new Padding(0, 0, 0, 6);
            lblTokenHint.Size = new Size(302, 21);
            lblTokenHint.TabIndex = 39;
            lblTokenHint.Text = "Tokens: {streamer} {date} {time} {game} {title} {streamId}";
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(308, 273);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(120, 23);
            btnStop.TabIndex = 28;
            btnStop.Text = "Stop";
            // 
            // txtClientId
            // 
            txtClientId.Location = new Point(182, 38);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(240, 23);
            txtClientId.TabIndex = 30;
            // 
            // txtClientSecret
            // 
            txtClientSecret.Location = new Point(182, 67);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.Size = new Size(240, 23);
            txtClientSecret.TabIndex = 31;
            txtClientSecret.UseSystemPasswordChar = true;
            // 
            // txtOutputDir
            // 
            txtOutputDir.Location = new Point(182, 96);
            txtOutputDir.Name = "txtOutputDir";
            txtOutputDir.Size = new Size(420, 23);
            txtOutputDir.TabIndex = 32;
            // 
            // nudDelayMinutes
            // 
            nudDelayMinutes.Location = new Point(182, 125);
            nudDelayMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            nudDelayMinutes.Name = "nudDelayMinutes";
            nudDelayMinutes.Size = new Size(120, 23);
            nudDelayMinutes.TabIndex = 34;
            nudDelayMinutes.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // nudDebouncePolls
            // 
            nudDebouncePolls.Location = new Point(182, 154);
            nudDebouncePolls.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nudDebouncePolls.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudDebouncePolls.Name = "nudDebouncePolls";
            nudDebouncePolls.Size = new Size(120, 23);
            nudDebouncePolls.TabIndex = 36;
            nudDebouncePolls.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // chkSplitOnGameChange
            // 
            chkSplitOnGameChange.AutoSize = true;
            chkSplitOnGameChange.Checked = true;
            chkSplitOnGameChange.CheckState = CheckState.Checked;
            chkSplitOnGameChange.Location = new Point(182, 183);
            chkSplitOnGameChange.Name = "chkSplitOnGameChange";
            chkSplitOnGameChange.Size = new Size(229, 19);
            chkSplitOnGameChange.TabIndex = 37;
            chkSplitOnGameChange.Text = "Split into new file when game changes";
            // 
            // txtFileTemplate
            // 
            txtFileTemplate.Location = new Point(182, 208);
            txtFileTemplate.Name = "txtFileTemplate";
            txtFileTemplate.Size = new Size(420, 23);
            txtFileTemplate.TabIndex = 38;
            txtFileTemplate.Text = "{streamer}_{date}_{time}_{game}";
            // 
            // lblTools
            // 
            lblTools.AutoSize = true;
            lblTools.Location = new Point(182, 255);
            lblTools.Name = "lblTools";
            lblTools.Size = new Size(46, 15);
            lblTools.TabIndex = 40;
            lblTools.Text = "Tools: -";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 12);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 47;
            label1.Text = "Streamer:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 41);
            label2.Name = "label2";
            label2.Size = new Size(93, 15);
            label2.TabIndex = 48;
            label2.Text = "Twitch Client ID:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 70);
            label3.Name = "label3";
            label3.Size = new Size(76, 15);
            label3.TabIndex = 49;
            label3.Text = "Client Secret:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 99);
            label4.Name = "label4";
            label4.Size = new Size(82, 15);
            label4.TabIndex = 50;
            label4.Text = "Output folder:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 127);
            label5.Name = "label5";
            label5.Size = new Size(145, 15);
            label5.TabIndex = 51;
            label5.Text = "Delay before record (min):";
            // 
            // label6
            // 
            label6.Location = new Point(6, 156);
            label6.Name = "label6";
            label6.Size = new Size(170, 33);
            label6.TabIndex = 52;
            label6.Text = "Game-change debounce (polls):";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(356, 127);
            label7.Name = "label7";
            label7.Size = new Size(100, 15);
            label7.TabIndex = 53;
            label7.Text = "Poll interval (sec):";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 211);
            label8.Name = "label8";
            label8.Size = new Size(108, 15);
            label8.TabIndex = 54;
            label8.Text = "Filename template:";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(967, 618);
            tabControl1.TabIndex = 56;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = SystemColors.Control;
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(lblTools);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(txtFileTemplate);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(chkSplitOnGameChange);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(nudDebouncePolls);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(nudDelayMinutes);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(txtOutputDir);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(txtClientSecret);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(txtClientId);
            tabPage1.Controls.Add(txtStreamer);
            tabPage1.Controls.Add(btnStop);
            tabPage1.Controls.Add(btnBrowse);
            tabPage1.Controls.Add(lblTokenHint);
            tabPage1.Controls.Add(nudPollSeconds);
            tabPage1.Controls.Add(btnStart);
            tabPage1.Controls.Add(lblStatus);
            tabPage1.Controls.Add(txtLog);
            tabPage1.Controls.Add(lblLiveFor);
            tabPage1.Controls.Add(lblFile);
            tabPage1.Controls.Add(lblGame);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(959, 590);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Recorder";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(_btnUnload);
            tabPage2.Controls.Add(_segments);
            tabPage2.Controls.Add(_videoView);
            tabPage2.Controls.Add(_lblTime);
            tabPage2.Controls.Add(_timeline);
            tabPage2.Controls.Add(_lblOut);
            tabPage2.Controls.Add(_lblIn);
            tabPage2.Controls.Add(_btnRemoveSegment);
            tabPage2.Controls.Add(_btnAddSegment);
            tabPage2.Controls.Add(_btnMarkOut);
            tabPage2.Controls.Add(_btnMarkIn);
            tabPage2.Controls.Add(_btnPlayPause);
            tabPage2.Controls.Add(_btnOpen);
            tabPage2.Controls.Add(_btnExport);
            tabPage2.Controls.Add(_txtOutput);
            tabPage2.Controls.Add(label9);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(959, 590);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Cutter";
            // 
            // _segments
            // 
            _segments.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
            _segments.FullRowSelect = true;
            _segments.Location = new Point(710, 35);
            _segments.Name = "_segments";
            _segments.Size = new Size(243, 512);
            _segments.TabIndex = 14;
            _segments.UseCompatibleStateImageBehavior = false;
            _segments.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Start";
            columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "End";
            columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Duration";
            columnHeader3.Width = 80;
            // 
            // _videoView
            // 
            _videoView.BackColor = Color.Black;
            _videoView.Location = new Point(20, 186);
            _videoView.MediaPlayer = null;
            _videoView.Name = "_videoView";
            _videoView.Size = new Size(684, 336);
            _videoView.TabIndex = 13;
            _videoView.Text = "videoView1";
            // 
            // _lblTime
            // 
            _lblTime.AutoSize = true;
            _lblTime.Location = new Point(24, 109);
            _lblTime.Name = "_lblTime";
            _lblTime.Size = new Size(144, 15);
            _lblTime.TabIndex = 12;
            _lblTime.Text = "00:00:00.000 / 00:00:00.000";
            // 
            // _timeline
            // 
            _timeline.Location = new Point(20, 127);
            _timeline.Maximum = 10000;
            _timeline.Name = "_timeline";
            _timeline.Size = new Size(684, 45);
            _timeline.TabIndex = 11;
            _timeline.TickStyle = TickStyle.None;
            // 
            // _lblOut
            // 
            _lblOut.AutoSize = true;
            _lblOut.Location = new Point(267, 109);
            _lblOut.Name = "_lblOut";
            _lblOut.Size = new Size(58, 15);
            _lblOut.TabIndex = 10;
            _lblOut.Text = "End cut: -";
            // 
            // _lblIn
            // 
            _lblIn.AutoSize = true;
            _lblIn.Location = new Point(263, 93);
            _lblIn.Name = "_lblIn";
            _lblIn.Size = new Size(62, 15);
            _lblIn.TabIndex = 9;
            _lblIn.Text = "Start cut: -";
            // 
            // _btnRemoveSegment
            // 
            _btnRemoveSegment.Location = new Point(295, 67);
            _btnRemoveSegment.Name = "_btnRemoveSegment";
            _btnRemoveSegment.Size = new Size(107, 23);
            _btnRemoveSegment.TabIndex = 8;
            _btnRemoveSegment.Text = "Remove segment";
            _btnRemoveSegment.UseVisualStyleBackColor = true;
            // 
            // _btnAddSegment
            // 
            _btnAddSegment.Location = new Point(182, 67);
            _btnAddSegment.Name = "_btnAddSegment";
            _btnAddSegment.Size = new Size(107, 23);
            _btnAddSegment.TabIndex = 7;
            _btnAddSegment.Text = "Add segment";
            _btnAddSegment.UseVisualStyleBackColor = true;
            // 
            // _btnMarkOut
            // 
            _btnMarkOut.Location = new Point(101, 67);
            _btnMarkOut.Name = "_btnMarkOut";
            _btnMarkOut.Size = new Size(75, 23);
            _btnMarkOut.TabIndex = 6;
            _btnMarkOut.Text = "End cut";
            _btnMarkOut.UseVisualStyleBackColor = true;
            // 
            // _btnMarkIn
            // 
            _btnMarkIn.Location = new Point(20, 67);
            _btnMarkIn.Name = "_btnMarkIn";
            _btnMarkIn.Size = new Size(75, 23);
            _btnMarkIn.TabIndex = 5;
            _btnMarkIn.Text = "Begin cut";
            _btnMarkIn.UseVisualStyleBackColor = true;
            // 
            // _btnPlayPause
            // 
            _btnPlayPause.Location = new Point(182, 38);
            _btnPlayPause.Name = "_btnPlayPause";
            _btnPlayPause.Size = new Size(75, 23);
            _btnPlayPause.TabIndex = 4;
            _btnPlayPause.Text = "Play";
            _btnPlayPause.UseVisualStyleBackColor = true;
            // 
            // _btnOpen
            // 
            _btnOpen.Location = new Point(20, 38);
            _btnOpen.Name = "_btnOpen";
            _btnOpen.Size = new Size(75, 23);
            _btnOpen.TabIndex = 3;
            _btnOpen.Text = "Open file";
            _btnOpen.UseVisualStyleBackColor = true;
            // 
            // _btnExport
            // 
            _btnExport.Location = new Point(446, 8);
            _btnExport.Name = "_btnExport";
            _btnExport.Size = new Size(75, 23);
            _btnExport.TabIndex = 2;
            _btnExport.Text = "Export";
            _btnExport.UseVisualStyleBackColor = true;
            // 
            // _txtOutput
            // 
            _txtOutput.Location = new Point(74, 9);
            _txtOutput.Name = "_txtOutput";
            _txtOutput.Size = new Size(366, 23);
            _txtOutput.TabIndex = 1;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(20, 12);
            label9.Name = "label9";
            label9.Size = new Size(48, 15);
            label9.TabIndex = 0;
            label9.Text = "Output:";
            // 
            // _btnUnload
            // 
            _btnUnload.Location = new Point(101, 38);
            _btnUnload.Name = "_btnUnload";
            _btnUnload.Size = new Size(75, 23);
            _btnUnload.TabIndex = 15;
            _btnUnload.Text = "Unload file";
            _btnUnload.UseVisualStyleBackColor = true;
            _btnUnload.Click += _btnUnload_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(991, 635);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Twitch Auto Recorder + Cutter | Created by Dekirai";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)nudPollSeconds).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDelayMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDebouncePolls).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_videoView).EndInit();
            ((System.ComponentModel.ISupportInitialize)_timeline).EndInit();
            ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private TextBox txtStreamer;
        private Button btnBrowse;
        private NumericUpDown nudPollSeconds;
        private Label lblStatus;
        private Label lblLiveFor;
        private Label lblGame;
        private Label lblFile;
        private TextBox txtLog;
        private Button btnStart;
        private Label lblTokenHint;
        private Button btnStop;
        private TextBox txtClientId;
        private TextBox txtClientSecret;
        private TextBox txtOutputDir;
        private NumericUpDown nudDelayMinutes;
        private NumericUpDown nudDebouncePolls;
        private CheckBox chkSplitOnGameChange;
        private TextBox txtFileTemplate;
        private Label lblTools;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox _txtOutput;
        private Label label9;
        private Button _btnExport;
        private Button _btnRemoveSegment;
        private Button _btnAddSegment;
        private Button _btnMarkOut;
        private Button _btnMarkIn;
        private Button _btnPlayPause;
        private Button _btnOpen;
        private Label _lblOut;
        private Label _lblIn;
        private TrackBar _timeline;
        private Label _lblTime;
        private LibVLCSharp.WinForms.VideoView _videoView;
        private ListView _segments;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button _btnUnload;
    }
}
