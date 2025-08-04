# PixelForge 

**Forge your images into any format with blazing speed**

A high-performance image converter and processor built with Rust. PixelForge transforms your images between multiple formats with support for batch processing, resizing, and quality control - all at incredible speeds. Available both as a command-line tool and Windows desktop application.

## Features

- **Blazing Fast**: Built with Rust for maximum performance
- **Batch Processing**: Convert entire directories of images in parallel
- **Multiple Formats**: Support for JPEG, PNG, WebP, GIF, BMP, and TIFF
- **Smart Resizing**: Resize images while converting with high-quality algorithms  
- **Quality Control**: Fine-tune compression settings for optimal results
- **Progress Tracking**: Beautiful progress bars for batch operations
- **Cross-platform CLI**: Works seamlessly on Windows, macOS, and Linux
- **Windows GUI**: User-friendly desktop application with clean interface

## Installation

### Command Line Tool

#### From Source
```bash
git clone https://github.com/lazzerex/pixel-forge.git
cd pixelforge/rust-cli
cargo build --release
```

The binary will be available at `target/release/pxforge`.

#### Using Cargo
```bash
cd rust-cli
cargo install --path .
```

### Windows Desktop Application

If you don't want to use the CLI, I have also made a WinForms application that is more straight-forward and easier to use.

#### Requirements
- .NET Framework 4.0 or later
- Windows 7 or later

#### Installation Steps
1. Clone this reposistory
2. Use Cargo to build the Rust project first (you can refer to the code above for installing with Cargo)
3. Build the WinForm application
3. Move the binary file (pxforge.exe) to .\PixelForgeUI\bin\Debug 
4. Open PixelForgeUI.sln and run the application

#### Building from Source
```bash
# First build the Rust CLI
cd rust-cli
cargo build --release
cp target/release/pxforge.exe ../windows-ui/

# Then build the Windows application
cd ../windows-ui
dotnet build --configuration Release
```

## Usage

### Command Line Interface

#### Basic Usage
```bash
# Convert single image
pxforge -i input.jpg -o output.png -f png

# Convert with quality setting  
pxforge -i photo.jpg -o photo.webp -f webp -q 90

# Resize while converting
pxforge -i large.png -o small.jpg -f jpg -r 800x600
```

#### Batch Processing
```bash
# Convert all images in a directory
pxforge -i ./photos -o ./converted -f webp --batch

# Batch convert with resizing
pxforge -i ./originals -o ./thumbnails -f jpg -r 200x200 --batch
```

#### Command Line Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--input` | `-i` | Input file or directory path | Required |
| `--output` | `-o` | Output file or directory path | Required |
| `--format` | `-f` | Output format (jpg, png, webp, gif, bmp, tiff) | `png` |
| `--quality` | `-q` | Quality for lossy formats (1-100) | `80` |
| `--resize` | `-r` | Resize image (WIDTHxHEIGHT) | None |
| `--batch` | `-b` | Process all images in input directory | `false` |
| `--strip-metadata` | | Strip metadata from images | `false` |
| `--force` | | Overwrite existing files without asking | `false` |
| `--verbose` | `-v` | Show detailed progress information | `false` |

### Windows Desktop Application

The Windows GUI provides an intuitive interface for all PixelForge features:

#### Getting Started
1. **Choose conversion mode**: Single file or batch folder processing
2. **Select input**: Browse for your image file or folder
3. **Pick format**: Choose your desired output format (PNG, JPEG, WebP, etc.)
4. **Set output location**: Choose where to save converted images
5. **Adjust settings**: Optional quality, resize, and metadata options
6. **Convert**: Click the convert button and watch real-time progress

#### Features
- **Smart file dialogs**: Automatically shows correct file types based on selected format
- **Real-time progress**: Live console output showing conversion progress
- **Batch processing**: Convert entire folders with progress tracking
- **Quality preview**: Adjust compression settings with live feedback
- **Error handling**: Clear error messages and validation
- **Intuitive workflow**: Logical step-by-step process guides users

## Examples

### Convert a single JPEG to PNG
```bash
pxforge -i photo.jpg -o photo.png
```

### Create WebP thumbnails from a directory of images
```bash
pxforge -i ./photos -o ./thumbnails -f webp -r 300x300 --batch
```

### High-quality JPEG conversion
```bash
pxforge -i input.png -o output.jpg -f jpg -q 95
```

### Batch convert with progress
```bash
pxforge -i ./raw_photos -o ./processed -f jpg -q 85 --batch --verbose
```

## Supported Formats

| Format | Extensions | Quality Control | Notes |
|--------|------------|-----------------|-------|
| JPEG | `.jpg`, `.jpeg` | Yes | Lossy compression |
| PNG | `.png` | No | Lossless compression |
| WebP | `.webp` | Yes | Modern format, great compression |
| GIF | `.gif` | No | Animation support |
| BMP | `.bmp` | No | Uncompressed |
| TIFF | `.tiff`, `.tif` | No | High quality, large files |

## Project Structure

```
pixelforge/
├── rust-cli/              # Rust command-line application
│   ├── src/
│   │   ├── main.rs
│   │   ├── cli.rs
│   │   ├── converter.rs
│   │   └── utils.rs
│   └── Cargo.toml
├── windows-ui/             # Windows Forms desktop application
│   ├── PixelForgeUI/
│   │   ├── Form1.cs
│   │   ├── Program.cs
│   │   └── PixelForgeUI.csproj
│   └── PixelForgeUI.sln
└── README.md
```

## Performance

PixelForge leverages Rust's performance and parallel processing for lightning-fast conversions:

- **Single image**: ~50-200ms depending on size and format
- **Batch processing**: Scales linearly with CPU cores (4-8x faster on multi-core systems)
- **Memory efficient**: Optimized for processing large batches without memory bloat
- **Zero-copy operations**: Minimal memory allocation during processing

## Contributing

We welcome contributions! Here's how you can help make PixelForge even better:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

- [ ] **Advanced Filters**: Blur, sharpen, brightness, contrast adjustments
- [ ] **Watermarking**: Add text or image watermarks
- [ ] **EXIF Handling**: Preserve or strip metadata with granular control
- [ ] **Animation Support**: Enhanced GIF and WebP animation processing
- [ ] **Presets**: Configuration files for common conversion workflows
- [ ] **macOS/Linux GUI**: Desktop applications for other platforms
- [ ] **Cloud Integration**: Direct upload/download from cloud storage
- [ ] **Plugin System**: Extensible architecture for custom processors

---

*Forge your pixels, unleash your creativity!*
