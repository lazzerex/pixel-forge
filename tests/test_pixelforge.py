#!/usr/bin/env python3
import os
import sys
import subprocess
import shutil
from pathlib import Path
import json
import time

class PixelForgeTestRunner:
    def __init__(self, project_path="."):
        self.project_path = Path(project_path).resolve()
        self.test_dir = self.project_path / "test_images"
        self.output_dir = self.project_path / "test_output"
        self.binary_path = None
        self.results = []
        
    def setup(self):
        self.test_dir.mkdir(exist_ok=True)
        if self.output_dir.exists():
            shutil.rmtree(self.output_dir)
        self.output_dir.mkdir()
        
        test_files = ["test.jpg", "test.png", "test.bmp", "large.png"]
        for f in test_files:
            with open(self.test_dir / f, 'wb') as file:
                file.write(b'\xFF\xD8\xFF\xE0' if f.endswith('.jpg') else b'\x89PNG\r\n\x1a\n')
                file.write(b'dummy_data' * 100)
    
    def build(self):
        try:
            subprocess.run(["cargo", "build", "--release"], cwd=self.project_path, check=True, capture_output=True)
            binary_name = "pxforge.exe" if os.name == "nt" else "pxforge"
            self.binary_path = self.project_path / "target" / "release" / binary_name
            return self.binary_path.exists()
        except:
            return False
    
    def run_cmd(self, args):
        try:
            result = subprocess.run([str(self.binary_path)] + args, capture_output=True, timeout=30)
            return result.returncode == 0
        except:
            return False
    
    def test(self, name, cmd_args, should_pass=True):
        start = time.time()
        success = self.run_cmd(cmd_args)
        duration = time.time() - start
        passed = success == should_pass
        self.results.append({"test": name, "passed": passed, "duration": duration})
        print(f"{'PASS' if passed else 'FAIL'} | {name} | {duration:.2f}s")
        return passed
    
    def run_tests(self):
        self.setup()
        if not self.build():
            print("Build failed")
            return False
        
        tests = [
            ("Help command", ["--help"]),
            ("Version command", ["--version"]),
            ("Single conversion", ["-i", str(self.test_dir/"test.jpg"), "-o", str(self.output_dir/"out.png")]),
            ("Batch conversion", ["-i", str(self.test_dir), "-o", str(self.output_dir/"batch"), "-b"]),
            ("With resize", ["-i", str(self.test_dir/"large.png"), "-o", str(self.output_dir/"resized.png"), "-r", "800x600"]),
            ("With quality", ["-i", str(self.test_dir/"test.jpg"), "-o", str(self.output_dir/"quality.jpg"), "-q", "50"]),
            ("Invalid input", ["-i", "nonexistent.jpg", "-o", str(self.output_dir/"fail.png")], False)
        ]
        
        for args in tests:
            self.test(*args)
        
        passed = sum(r["passed"] for r in self.results)
        total = len(self.results)
        print(f"\nResults: {passed}/{total} passed ({passed/total*100:.1f}%)")
        
        with open(self.project_path / "test_report.json", 'w') as f:
            json.dump({"summary": {"passed": passed, "total": total}, "results": self.results}, f)
        
        shutil.rmtree(self.test_dir)
        shutil.rmtree(self.output_dir)
        return passed == total

if __name__ == "__main__":
    project_path = sys.argv[1] if len(sys.argv) > 1 else "."
    runner = PixelForgeTestRunner(project_path)
    success = runner.run_tests()
    sys.exit(0 if success else 1)