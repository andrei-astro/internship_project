# Installation and Usage Guide

## Prerequisites

1. **Install .NET 8.0 SDK**
   - **macOS**: 
     ```bash
     # Using Homebrew
     brew install --cask dotnet
     
     # Or download from Microsoft
     # Visit: https://dotnet.microsoft.com/download/dotnet/8.0
     ```
   
   - **Windows**: 
     - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
     - Run the installer and follow the setup wizard
   
   - **Linux**: 
     ```bash
     # Ubuntu/Debian
     sudo apt-get update
     sudo apt-get install -y dotnet-sdk-8.0
     
     # Or visit: https://docs.microsoft.com/en-us/dotnet/core/install/linux
     ```

2. **Verify Installation**
   ```bash
   dotnet --version
   # Should output something like: 8.0.x
   ```

## Quick Start

1. **Clone or Download the Project**
   ```bash
   # Navigate to the project directory
   cd /path/to/RoslynParameterDuplicator
   ```

2. **Build the Project**
   ```bash
   dotnet build
   ```

3. **Run the Application**
   ```bash
   # Basic usage
   dotnet run <input-file> <output-file>
   
   # Example with sample file
   dotnet run SampleInput.cs ModifiedOutput.cs
   ```

4. **Run Tests** (Optional)
   ```bash
   # On macOS/Linux
   ./test.sh
   
   # On Windows (PowerShell)
   .\test.ps1
   ```

## Command Line Usage

```bash
dotnet run <input_file.cs> <output_file.cs>
```

### Arguments:
- `input_file.cs`: Path to the C# source file to process
- `output_file.cs`: Path where the modified source file will be saved

### Examples:
```bash
# Process a single file
dotnet run Program.cs ModifiedProgram.cs

# Process with absolute paths
dotnet run /path/to/source.cs /path/to/output.cs

# Process sample file included in project
dotnet run SampleInput.cs SampleOutput.cs
```

## What the Program Does

The program will:
1. ✅ Find all method declarations with exactly **one parameter**
2. ✅ Duplicate that parameter with an intelligently suggested name
3. ✅ Preserve all original formatting, comments, and code structure
4. ✅ Leave methods with 0 or multiple parameters unchanged

## Example Transformations

| Original | Transformed | Suggestion Logic |
|----------|-------------|------------------|
| `Method(int value)` | `Method(int value, int newValue)` | Semantic suggestion |
| `Method(string input)` | `Method(string input, string secondInput)` | Semantic suggestion |
| `Method(bool isActive)` | `Method(bool isActive, bool shouldActive)` | Prefix-based |
| `Method(object item1)` | `Method(object item1, object item2)` | Number increment |
| `Method(User user)` | `Method(User user, User alternativeUser)` | Default strategy |

## Troubleshooting

- **"dotnet command not found"**: Install .NET SDK (see Prerequisites)
- **Build errors**: Ensure you're using .NET 8.0 or compatible version
- **File not found**: Check input file path is correct and file exists
- **Permission errors**: Ensure write permissions to output directory