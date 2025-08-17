#!/usr/bin/env python3
"""
PixelForge Pure Python Testing Script
No external dependencies required - uses only Python standard library
"""

import os
import sys
import subprocess
import tempfile
import shutil
from pathlib import Path
import argparse
import json
import time
from datetime import datetime

class PixelForgeTestRunner:
    def __init__(self, project_path: str = "."):
        self.project_path = Path(project_path).resolve()
        self.test_dir = self.project_path / "test_images"
        self.output_dir = self.project_path / "test_output"
        self.binary_path = None
        self.test_results = []
        
    def setup_test_environment(self):
        """Set up test directories and create simple test files"""
        print("Setting up test environment...")
        
        
        self.test_dir.mkdir(exist_ok=True)
        self.output_dir.mkdir(exist_ok=True)
        
        
        if self.output_dir.exists():
            shutil.rmtree(self.output_dir)
        self.output_dir.mkdir()
        
        
        self._create_test_files()
        print("Test environment ready")
    
    def _create_test_files(self):
        """Create simple test files to simulate images"""
        
        test_files = [
            "test_image.jpg",
            "test_image.png", 
            "test_image.bmp",
            "large_image.png",
            "small_image.gif"
        ]
        
        for filename in test_files:
            file_path = self.test_dir / filename
            
            with open(file_path, 'wb') as f:
                
                f.write(b'\xFF\xD8\xFF\xE0' if filename.endswith('.jpg') else b'\x89PNG\r\n\x1a\n')
                f.write(b'dummy_image_data_for_testing' * 100)  # Make it reasonably sized
            
            print(f"Created test file: {filename} ({file_path.stat().st_size} bytes)")
    
    def build_project(self):
        """Build the Rust project"""
        print("Building Rust project...")
        
        try:
            # release mode
            result = subprocess.run(
                ["cargo", "build", "--release"],
                cwd=self.project_path,
                capture_output=True,
                text=True,
                check=True
            )
            
            
            target_dir = self.project_path / "target" / "release"
            binary_name = "pxforge"
            if os.name == "nt":  # Windows
                binary_name += ".exe"
                
            self.binary_path = target_dir / binary_name
            
            if not self.binary_path.exists():
                raise FileNotFoundError(f"Binary not found at {self.binary_path}")
                
            print(f"Build successful. Binary at: {self.binary_path}")
            return True
            
        except subprocess.CalledProcessError as e:
            print(f"Build failed:")
            print(f"STDOUT: {e.stdout}")
            print(f"STDERR: {e.stderr}")
            return False
        except Exception as e:
            print(f"Build error: {e}")
            return False
    
    def test_binary_exists(self):
        """Test if the binary was built successfully"""
        test_name = "Binary existence check"
        print(f"Running: {test_name}")
        
        passed = self.binary_path and self.binary_path.exists()
        
        self.test_results.append({
            "test": test_name,
            "passed": passed,
            "duration": 0,
            "binary_path": str(self.binary_path) if self.binary_path else "Not found"
        })
        
        status = "PASS" if passed else "FAIL"
        print(f"  {status}")
        return passed
    
    def test_help_command(self):
        """Test the help command"""
        test_name = "Help command test"
        print(f"Running: {test_name}")
        
        start_time = time.time()
        success = self._run_converter_command(["--help"])
        duration = time.time() - start_time
        
        self.test_results.append({
            "test": test_name,
            "passed": success,
            "duration": duration
        })
        
        status = "PASS" if success else "FAIL"
        print(f"  {status} ({duration:.2f}s)")
        return success
    
    def test_version_command(self):
        """Test the version command"""
        test_name = "Version command test"
        print(f"Running: {test_name}")
        
        start_time = time.time()
        success = self._run_converter_command(["--version"])
        duration = time.time() - start_time
        
        self.test_results.append({
            "test": test_name,
            "passed": success,
            "duration": duration
        })
        
        status = "PASS" if success else "FAIL"
        print(f"  {status} ({duration:.2f}s)")
        return success
    
    def test_single_file_conversion(self):
        """Test single file conversion (command execution only)"""
        test_name = "Single file conversion test"
        print(f"Running: {test_name}")
        
        input_file = self.test_dir / "test_image.jpg"
        output_file = self.output_dir / "converted_image.png"
        
        start_time = time.time()
        success = self._run_converter_command([
            "-i", str(input_file),
            "-o", str(output_file),
            "-f", "png"
        ])
        duration = time.time() - start_time
        
       
        output_created = output_file.exists()
        
        test_passed = success and output_created
        
        self.test_results.append({
            "test": test_name,
            "passed": test_passed,
            "duration": duration,
            "output_created": output_created,
            "command_success": success
        })
        
        status = "PASS" if test_passed else "FAIL"
        print(f"  {status} ({duration:.2f}s) - Output created: {output_created}")
        return test_passed
    
    def test_batch_conversion(self):
        """Test batch conversion"""
        test_name = "Batch conversion test"
        print(f"Running: {test_name}")
        
        batch_output = self.output_dir / "batch_output"
        batch_output.mkdir(exist_ok=True)
        
        start_time = time.time()
        success = self._run_converter_command([
            "-i", str(self.test_dir),
            "-o", str(batch_output),
            "-f", "png",
            "-b"
        ])
        duration = time.time() - start_time
        
        
        output_files = list(batch_output.rglob("*"))
        input_files = list(self.test_dir.glob("*"))
        
        self.test_results.append({
            "test": test_name,
            "passed": success,
            "duration": duration,
            "input_files": len(input_files),
            "output_files": len(output_files),
            "command_success": success
        })
        
        status = "PASS" if success else "FAIL"
        print(f"  {status} ({duration:.2f}s) - {len(output_files)} files created")
        return success
    
    def test_resize_parameter(self):
        """Test resize parameter"""
        test_name = "Resize parameter test"
        print(f"Running: {test_name}")
        
        input_file = self.test_dir / "large_image.png"
        output_file = self.output_dir / "resized_image.png"
        
        start_time = time.time()
        success = self._run_converter_command([
            "-i", str(input_file),
            "-o", str(output_file),
            "-r", "800x600"
        ])
        duration = time.time() - start_time
        
        output_created = output_file.exists()
        
        self.test_results.append({
            "test": test_name,
            "passed": success and output_created,
            "duration": duration,
            "output_created": output_created
        })
        
        status = "PASS" if success and output_created else "FAIL"
        print(f"  {status} ({duration:.2f}s)")
        return success and output_created
    
    def test_quality_parameter(self):
        """Test quality parameter"""
        test_name = "Quality parameter test"
        print(f"Running: {test_name}")
        
        input_file = self.test_dir / "test_image.jpg"
        output_file = self.output_dir / "quality_test.jpg"
        
        start_time = time.time()
        success = self._run_converter_command([
            "-i", str(input_file),
            "-o", str(output_file),
            "-f", "jpg",
            "-q", "50"
        ])
        duration = time.time() - start_time
        
        output_created = output_file.exists()
        
        self.test_results.append({
            "test": test_name,
            "passed": success and output_created,
            "duration": duration,
            "output_created": output_created
        })
        
        status = "PASS" if success and output_created else "FAIL"
        print(f"  {status} ({duration:.2f}s)")
        return success and output_created
    
    def test_invalid_input(self):
        """Test behavior with invalid input"""
        test_name = "Invalid input handling test"
        print(f"Running: {test_name}")
        
        # Test with non-existent file
        start_time = time.time()
        success = self._run_converter_command([
            "-i", "nonexistent_file.jpg",
            "-o", str(self.output_dir / "should_not_exist.png")
        ])
        duration = time.time() - start_time
        
        # (success = False)
        test_passed = not success
        
        self.test_results.append({
            "test": test_name,
            "passed": test_passed,
            "duration": duration,
            "expected_failure": True,
            "actual_failure": not success
        })
        
        status = "PASS" if test_passed else "FAIL"
        print(f"  {status} ({duration:.2f}s) - Correctly handled invalid input")
        return test_passed
    
    def test_verbose_mode(self):
        """Test verbose mode"""
        test_name = "Verbose mode test"
        print(f"Running: {test_name}")
        
        input_file = self.test_dir / "test_image.png"
        output_file = self.output_dir / "verbose_test.jpg"
        
        start_time = time.time()
        success = self._run_converter_command([
            "-i", str(input_file),
            "-o", str(output_file),
            "-v"
        ])
        duration = time.time() - start_time
        
        self.test_results.append({
            "test": test_name,
            "passed": success,
            "duration": duration
        })
        
        status = "PASS" if success else "FAIL"
        print(f"  {status} ({duration:.2f}s)")
        return success
    
    def _run_converter_command(self, args):
        """Run the converter with given arguments"""
        if not self.binary_path:
            print("Binary not found. Build the project first.")
            return False
        
        try:
            cmd = [str(self.binary_path)] + args
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=30  # 30 second timeout
            )
            
            # print(f"Command: {' '.join(cmd)}")
            # print(f"Return code: {result.returncode}")
            # if result.stdout: print(f"STDOUT: {result.stdout}")
            # if result.stderr: print(f"STDERR: {result.stderr}")
            
            return result.returncode == 0
            
        except subprocess.TimeoutExpired:
            print("  Command timed out")
            return False
        except Exception as e:
            print(f"  Command failed: {e}")
            return False
    
    def run_file_size_tests(self):
        """Test different file operations and sizes"""
        test_name = "File size handling test"
        print(f"Running: {test_name}")
        
        
        small_file = self.test_dir / "tiny.jpg"
        large_file = self.test_dir / "huge.png"
        
        
        with open(small_file, 'wb') as f:
            f.write(b'\xFF\xD8\xFF\xE0small')
        
        
        with open(large_file, 'wb') as f:
            f.write(b'\x89PNG\r\n\x1a\n')
            f.write(b'large_dummy_data' * 10000)  # ~160KB
        
        tests_passed = 0
        total_tests = 2
        
    
        start_time = time.time()
        success1 = self._run_converter_command([
            "-i", str(small_file),
            "-o", str(self.output_dir / "small_converted.png")
        ])
        if success1:
            tests_passed += 1
        
      
        success2 = self._run_converter_command([
            "-i", str(large_file),
            "-o", str(self.output_dir / "large_converted.jpg")
        ])
        if success2:
            tests_passed += 1
        
        duration = time.time() - start_time
        test_passed = tests_passed == total_tests
        
        self.test_results.append({
            "test": test_name,
            "passed": test_passed,
            "duration": duration,
            "subtests_passed": f"{tests_passed}/{total_tests}"
        })
        
        status = "PASS" if test_passed else "FAIL"
        print(f"  {status} ({duration:.2f}s) - {tests_passed}/{total_tests} size tests passed")
        return test_passed
    
    def generate_test_report(self):
        """Generate a comprehensive test report"""
        print("\n" + "="*60)
        print("PIXELFORGE TEST REPORT")
        print("="*60)
        
        total_tests = len(self.test_results)
        passed_tests = sum(1 for result in self.test_results if result["passed"])
        
        print(f"Total tests run: {total_tests}")
        print(f"Tests passed: {passed_tests}")
        print(f"Tests failed: {total_tests - passed_tests}")
        print(f"Success rate: {(passed_tests/total_tests)*100:.1f}%")
        
        print("\nDETAILED RESULTS:")
        print("-" * 60)
        
        for result in self.test_results:
            status = "PASS" if result["passed"] else "FAIL"
            duration = result.get("duration", 0)
            print(f"{status:4} | {result['test']:35} | {duration:.3f}s")
            
           
            if "output_created" in result:
                print(f"     | Output file created: {result['output_created']}")
            if "subtests_passed" in result:
                print(f"     | Sub-tests: {result['subtests_passed']}")
            if "binary_path" in result:
                print(f"     | Binary: {result['binary_path']}")
        
      
        report_file = self.project_path / "test_report.json"
        with open(report_file, 'w') as f:
            json.dump({
                "timestamp": datetime.now().isoformat(),
                "summary": {
                    "total_tests": total_tests,
                    "passed_tests": passed_tests,
                    "failed_tests": total_tests - passed_tests,
                    "success_rate": (passed_tests/total_tests)*100 if total_tests > 0 else 0
                },
                "detailed_results": self.test_results
            }, f, indent=2)
        
        print(f"\nDetailed report saved to: {report_file}")
        
        return passed_tests == total_tests
    
    def cleanup(self):
        """Clean up test files"""
        print("\nCleaning up test files...")
        if self.test_dir.exists():
            shutil.rmtree(self.test_dir)
        if self.output_dir.exists():
            shutil.rmtree(self.output_dir)
    
    def run_all_tests(self, cleanup_after=True):
        """Run all tests"""
        try:
            self.setup_test_environment()
            
            if not self.build_project():
                print("Build failed. Cannot run tests.")
                return False
            
            
            self.test_binary_exists()
            self.test_help_command()
            self.test_version_command()
            self.test_single_file_conversion()
            self.test_batch_conversion()
            self.test_resize_parameter()
            self.test_quality_parameter()
            self.test_invalid_input()
            self.test_verbose_mode()
            self.run_file_size_tests()
            
            success = self.generate_test_report()
            
            if cleanup_after:
                self.cleanup()
            
            return success
            
        except KeyboardInterrupt:
            print("\nTests interrupted by user")
            return False
        except Exception as e:
            print(f"\nTest runner error: {e}")
            return False

def main():
    parser = argparse.ArgumentParser(description="Test PixelForge image converter (Pure Python)")
    parser.add_argument("--project-path", "-p", default=".", 
                       help="Path to the Rust project (default: current directory)")
    parser.add_argument("--no-cleanup", action="store_true",
                       help="Keep test files after running tests")
    parser.add_argument("--build-only", action="store_true",
                       help="Only build the project, don't run tests")
    
    args = parser.parse_args()
    
    test_runner = PixelForgeTestRunner(args.project_path)
    
    if args.build_only:
        success = test_runner.build_project()
        print("Build completed successfully!" if success else "Build failed!")
        return 0 if success else 1
    
    success = test_runner.run_all_tests(cleanup_after=not args.no_cleanup)
    
    if success:
        print("\nAll tests passed! PixelForge is working correctly.")
        return 0
    else:
        print("\nSome tests failed. Check the report above.")
        return 1

if __name__ == "__main__":
    sys.exit(main())