use anyhow::{anyhow, Result};
use image::{ImageFormat, ImageOutputFormat, DynamicImage};
use indicatif::{ProgressBar, ProgressStyle};
use rayon::prelude::*;
use std::path::{Path, PathBuf};
use walkdir::WalkDir;

use crate::utils::{get_image_format, parse_dimensions, get_output_path};

pub struct ImageConverter {
    quality: u8,
    resize: Option<(u32, u32)>,
}

impl ImageConverter {
    pub fn new(quality: u8, resize: Option<String>) -> Result<Self> {
        let resize_dims = if let Some(resize_str) = resize {
            Some(parse_dimensions(&resize_str)?)
        } else {
            None
        };

        Ok(Self {
            quality: quality.clamp(1, 100),
            resize: resize_dims,
        })
    }

    pub fn convert_single(&self, input: &Path, output: &Path, format: &str) -> Result<()> {
        println!("Converting {} -> {}", input.display(), output.display());
        
        let img = image::open(input)
            .map_err(|e| anyhow!("Failed to open image {}: {}", input.display(), e))?;

        let processed_img = self.process_image(img)?;
        self.save_image(processed_img, output, format)?;
        
        println!("✓ Conversion complete!");
        Ok(())
    }

    pub fn convert_batch(&self, input: &Path, output: &Path, format: &str) -> Result<()> {
        let image_files: Vec<PathBuf> = WalkDir::new(input)
            .into_iter()
            .filter_map(|entry| entry.ok())
            .filter(|entry| entry.file_type().is_file())
            .filter(|entry| {
                entry.path().extension()
                    .and_then(|ext| ext.to_str())
                    .map(|ext| matches!(ext.to_lowercase().as_str(), 
                        "jpg" | "jpeg" | "png" | "gif" | "bmp" | "tiff" | "webp"))
                    .unwrap_or(false)
            })
            .map(|entry| entry.path().to_path_buf())
            .collect();

        if image_files.is_empty() {
            return Err(anyhow!("No image files found in {}", input.display()));
        }

        println!("Found {} images to convert", image_files.len());

        let pb = ProgressBar::new(image_files.len() as u64);
        pb.set_style(
            ProgressStyle::default_bar()
                .template("{spinner:.green} [{elapsed_precise}] [{bar:40.cyan/blue}] {pos}/{len} {msg}")
                .unwrap()
                .progress_chars("#>-"),
        );

        let results: Vec<Result<()>> = image_files
            .par_iter()
            .map(|input_file| {
                let output_file = get_output_path(input_file, input, output, format);
                let result = self.convert_file(input_file, &output_file, format);
                pb.inc(1);
                result
            })
            .collect();

        pb.finish_with_message("Batch conversion complete!");

        let errors: Vec<_> = results.into_iter().filter_map(|r| r.err()).collect();
        if !errors.is_empty() {
            println!("Encountered {} errors:", errors.len());
            for error in errors {
                println!("  ✗ {}", error);
            }
        }

        Ok(())
    }

    fn convert_file(&self, input: &Path, output: &Path, format: &str) -> Result<()> {
        
        if let Some(parent) = output.parent() {
            std::fs::create_dir_all(parent)?;
        }

        let img = image::open(input)?;
        let processed_img = self.process_image(img)?;
        self.save_image(processed_img, output, format)?;
        Ok(())
    }

    fn process_image(&self, mut img: DynamicImage) -> Result<DynamicImage> {
        
        if let Some((width, height)) = self.resize {
            img = img.resize(width, height, image::imageops::FilterType::Lanczos3);
        }

        Ok(img)
    }

    fn save_image(&self, img: DynamicImage, output: &Path, format: &str) -> Result<()> {
        let format = get_image_format(format)?;
        
        match format {
            ImageFormat::Jpeg => {
                let output_format = ImageOutputFormat::Jpeg(self.quality);
                img.write_to(&mut std::fs::File::create(output)?, output_format)?;
            }
            ImageFormat::WebP => {
                
                img.save_with_format(output, format)?;
            }
            _ => {
                img.save_with_format(output, format)?;
            }
        }

        Ok(())
    }
}