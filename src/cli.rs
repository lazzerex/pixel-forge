use clap::Parser;
use std::path::PathBuf;

#[derive(Parser)]
#[command(name = "pxforge")]
#[command(about = "PixelForge - Forge your images into any format with blazing speed")]
#[command(version = "0.1.0")]
#[command(author = "Your Name")]
#[command(long_about = "PixelForge is a high-performance image converter built with Rust. 
Transform your images between formats, resize them, and process batches with incredible speed.")]
pub struct Args {
    
    #[arg(short, long)]
    pub input: PathBuf,

    
    #[arg(short, long)]
    pub output: PathBuf,

    
    #[arg(short, long, default_value = "png")]
    pub format: String,

    
    #[arg(short, long, default_value = "80")]
    pub quality: u8,

    
    #[arg(short, long)]
    pub resize: Option<String>,

    
    #[arg(short, long)]
    pub batch: bool,

    
    #[arg(long)]
    pub strip_metadata: bool,

    
    #[arg(long)]
    pub force: bool,

    
    #[arg(short, long)]
    pub verbose: bool,
}