use clap::Parser;
use anyhow::Result;

mod cli;
mod converter;
mod utils;

use cli::Args;
use converter::ImageConverter;

fn main() -> Result<()> {
    let args = Args::parse();
    
    let converter = ImageConverter::new(args.quality, args.resize)?;
    
    if args.batch {
        converter.convert_batch(&args.input, &args.output, &args.format)?;
    } else {
        converter.convert_single(&args.input, &args.output, &args.format)?;
    }
    
    Ok(())
}