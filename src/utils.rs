use anyhow::{anyhow, Result};
use image::ImageFormat;
use std::path::{Path, PathBuf};

pub fn get_image_format(format_str: &str) -> Result<ImageFormat> {
    match format_str.to_lowercase().as_str() {
        "jpg" | "jpeg" => Ok(ImageFormat::Jpeg),
        "png" => Ok(ImageFormat::Png),
        "gif" => Ok(ImageFormat::Gif),
        "bmp" => Ok(ImageFormat::Bmp),
        "tiff" | "tif" => Ok(ImageFormat::Tiff),
        "webp" => Ok(ImageFormat::WebP),
        _ => Err(anyhow!("Unsupported format: {}", format_str)),
    }
}

pub fn parse_dimensions(dimensions: &str) -> Result<(u32, u32)> {
    let parts: Vec<&str> = dimensions.split('x').collect();
    if parts.len() != 2 {
        return Err(anyhow!("Invalid dimensions format. Use WIDTHxHEIGHT (e.g., 800x600)"));
    }

    let width: u32 = parts[0].parse()
        .map_err(|_| anyhow!("Invalid width: {}", parts[0]))?;
    let height: u32 = parts[1].parse()
        .map_err(|_| anyhow!("Invalid height: {}", parts[1]))?;

    if width == 0 || height == 0 {
        return Err(anyhow!("Width and height must be greater than 0"));
    }

    Ok((width, height))
}

pub fn get_output_path(
    input_file: &Path,
    input_dir: &Path,
    output_dir: &Path,
    format: &str,
) -> PathBuf {
    let relative_path = input_file.strip_prefix(input_dir).unwrap();
    let mut output_path = output_dir.join(relative_path);
    
    
    output_path.set_extension(format);
    output_path
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_parse_dimensions() {
        assert_eq!(parse_dimensions("800x600").unwrap(), (800, 600));
        assert_eq!(parse_dimensions("1920x1080").unwrap(), (1920, 1080));
        assert!(parse_dimensions("800").is_err());
        assert!(parse_dimensions("800x").is_err());
        assert!(parse_dimensions("800x0").is_err());
    }

    #[test]
    fn test_get_image_format() {
        assert!(matches!(get_image_format("jpg").unwrap(), ImageFormat::Jpeg));
        assert!(matches!(get_image_format("PNG").unwrap(), ImageFormat::Png));
        assert!(get_image_format("xyz").is_err());
    }
}