# Twitch Auto Recorder & Cutter

A Windows **C# WinForms** application that automatically records Twitch streams **when they go live**, splits recordings, and provides a **lossless video cutter** with live preview.

This project is designed for **archiving and editing Twitch streams with zero quality loss**.

---

## Preview

![Recorder UI](https://i.imgur.com/EGdoduq.png)
![Lossless cutting demo](https://i.imgur.com/kxDt2Hw.gif)

---

## Features

- Monitor a single streamer by **login name** (e.g. `H0llyLP`)
- Detects **live / offline**
- Optional **delay before recording starts** (minutes)
- Records via **Streamlink**
- Uses bundled **FFmpeg** for lossless remux
- Outputs finalized **MP4**
- Optional: **split into a new file when the game changes**
  - Configurable poll interval (seconds)
  - Configurable debounce (number of consecutive polls)
  - May be bugged, not really tested yet
- Live label: shows **how long the stream has been live**
- Filename templates with tokens:
  - `{streamer}` `{date}` `{time}` `{game}` `{title}` `{streamId}`
- Sanitizes filenames (invalid characters replaced) and trims length

---

## How it works (pipeline)

1. The app polls Twitch Helix `Get Streams` for the streamer.
2. When live is detected:
   - waits the configured delay
   - starts recording with Streamlink into a `.ts` file
3. When stopping (manual stop or stream ends):
   - FFmpeg remuxes `.ts` → `.mp4` using stream copy (`-c copy`)
4. If “Split on game change” is enabled:
   - the app restarts Streamlink into a new `.ts`
   - finalizes the previous segment to `.mp4`

**No re-encoding** is performed for MP4 finalization, so quality stays identical to the stream.

---

## Requirements

- Windows 10/11
- **.NET 8** (project targets `net8.0-windows`)
- **Streamlink installed and available in PATH**
- FFmpeg bundled in the app folder:
  - `Tools/ffmpeg/ffmpeg.exe`

---

## Install / Setup

### 1) Install Streamlink
Install Streamlink so this works in Command Prompt:

```bat
streamlink --version
```

### 2) FFmpeg
Place `ffmpeg.exe` here (relative to the app executable):

```
Tools/ffmpeg/ffmpeg.exe
```

The app will show tool status in the UI: `Tools: ffmpeg=OK | streamlink=OK (PATH)`.

---

## Twitch API credentials (Client Credentials)

This app uses the **Client Credentials** OAuth flow (no user login required).

### Create a Twitch App
1. Open the Twitch developer console and create an application.
2. Copy **Client ID** and **Client Secret**.
3. Paste them into the app fields.

### OAuth Redirect URL
The current code does **not** use an interactive redirect, but Twitch requires a value when creating an app.
Use:

```
http://localhost
```

---

## Usage

1. Enter:
   - Streamer login (e.g. `h0llylp`)
   - Twitch Client ID
   - Twitch Client Secret
   - Output folder
2. Configure:
   - Delay before record (min)
   - Poll interval (sec)
   - (Optional) Split on game change + debounce
   - Filename template
3. Click **Start**.

## Video Cutter
- Dedicated **Cutter tab**
- Live preview (LibVLCSharp)
- Timeline scrubbing
- Mark **Start cut / End cut**
- Add multiple **cut-out ranges**
- Removes selected parts (keeps everything else)
- **No re-encoding / no quality loss**
- TS-intermediate export to prevent freezes or FPS issues

## Cutter Workflow

1. Open MP4
2. Mark sections to REMOVE
3. Add to cut list
4. Export
5. Get a clean MP4 without quality loss

## Notes

- Lossless cuts are keyframe-limited
- Small timing shifts are expected
- This is required for zero quality loss

### Output files

Recordings are stored under:

```
<OutputFolder>\<streamer>\<filename>.mp4
```

During recording, Streamlink writes a raw `.ts`. After finalize, you get `.mp4`.

---

## Filename template tokens

You can use these tokens in the “Filename template” field:

- `{streamer}` – streamer name
- `{date}` – `yyyy-MM-dd`
- `{time}` – `HH-mm-ss`
- `{game}` – current game/category name
- `{title}` – current stream title
- `{streamId}` – Twitch stream id

Example:

```
{streamer}_{date}_{time}_{game}
```

---

## Notes on splitting by game

Twitch “game/category” can briefly flicker or update slowly.  
To avoid false splits, the app uses **debounce**:

- A new game must be detected for **N consecutive polls** before a split happens.

---

## Legal / ethical notice

Recording streams may violate Twitch Terms or a creator’s rights depending on your usage.  
Use this tool only for **personal archival / permission-based use**, and comply with:

- Twitch Terms of Service
- Copyright law
- Streamer consent/rights

---

## Credits

Created by **Dekirai**  
Built with C#, Streamlink, FFmpeg, and Twitch API.
