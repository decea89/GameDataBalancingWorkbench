# Demo GIF Recording Instructions

This guide explains how to create a demonstration GIF for BalanceForge that showcases the MVP workflow.

## Tools Needed

- **ScreenToGif** (free): https://www.screentogif.com/ - Recommended, simple to use
- OR **FFmpeg** (command-line, free): https://ffmpeg.org/
- OR **OBS Studio** (free): https://obsproject.com/

## Recommended Workflow (ScreenToGif)

### Setup
1. Install ScreenToGif from https://www.screentogif.com/
2. Launch the application
3. Select "Recorder" window
4. Position the recorder to capture a 1280x720 or 1400x800 region

### Recording Steps (Total: ~45 seconds)

1. **[0s - 5s] Start with empty UI**
   - Show the BalanceForge window with no file loaded
   - Show the toolbar, empty grid, empty inspector

2. **[5s - 15s] Load sample data**
   - Click "Select File"
   - Navigate to `samples/units.json`
   - Click "Load"
   - Wait for the roster to populate
   - Pause to let viewer see the 4 units in the grid

3. **[15s - 25s] View chart and compare**
   - Scroll or observe the Balance Chart showing Total Cost, DPS, Effective Health
   - Click on "Knight" in the grid
   - Ctrl+Click on "Spearman" to populate the Comparison panel
   - Show both units side-by-side in the comparison view

4. **[25s - 35s] Edit and validate**
   - In the inspector, edit Knight's Health value (change 180 → 250)
   - Show validation warnings appearing in the Issues panel
   - Highlight the "out of tier range" warning for Knight

5. **[35s - 40s] Undo and save**
   - Press Ctrl+Z to undo the edit
   - Show Knight's Health return to 180 and warnings disappear
   - Click "Save" button to demonstrate save workflow

6. **[40s - 45s] End screen**
   - Show final UI state with all features visible

### Export Settings
- **Format**: MP4 or GIF (GIF for web, MP4 for better compression)
- **Resolution**: 1280x720 or 1400x800
- **Frame rate**: 30 FPS
- **Quality**: High

### Post-Processing (Optional)
- Crop to remove unnecessary UI elements
- Add text overlays or captions if desired
- Reduce file size using FFmpeg if needed

## Command-line Alternative (FFmpeg)

If you prefer FFmpeg:

```bash
# Record screen region to video
ffmpeg -f gdigrab -framerate 30 -i desktop -vf "crop=1400:800:0:0" demo.mp4

# Convert MP4 to GIF
ffmpeg -i demo.mp4 -pix_fmt rgb24 -r 30 demo.gif

# Optimize GIF size
ffmpeg -i demo.mp4 -vf "fps=10,scale=1280:-1:flags=lanczos,split[s0][s1];[s0]palgen[p];[s1][p]paletteuse" -loop 0 demo.gif
```

## File Location and README Integration

1. Save the GIF as `docs/demo.gif`
2. Update README.md to reference it in the main section:

```markdown
## Demo

![BalanceForge Demo: Load sample data, compare units, edit with validation, undo and save](docs/demo.gif)
```

This will display the GIF on the GitHub README and in the VS Code preview.

## Troubleshooting

- **GIF file too large?** Use FFmpeg optimization or reduce frame rate to 10 FPS
- **Performance lag?** Record at a lower resolution or close unnecessary background apps
- **Audio issues?** GIFs don't support audio; focus on visual flow

---

Created as part of the MVP completion checklist for BalanceForge.
