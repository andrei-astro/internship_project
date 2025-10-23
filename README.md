# Roslyn Parameter Duplicator

This C# console application uses Microsoft Roslyn SDK to automatically find method declarations with a single parameter and duplicate that parameter with a suggestive name.

## Features

- Parses C# source code files using Roslyn syntax trees
- Identifies method declarations with exactly one parameter
- Duplicates the parameter with a suggestive name
- Preserves original code formatting and structure
- Supports various parameter types and modifiers

## Parameter Name Suggestion Algorithm

The program uses several strategies to suggest meaningful names for duplicated parameters:

1. **Semantic Suggestions**: Common parameter names get contextually appropriate alternatives:
   - `value` → `newValue`
   - `data` → `additionalData`
   - `input` → `secondInput`
   - `source` → `destination`

2. **Prefix-based Suggestions**:
   - `isActive` → `shouldBeActive`
   - `hasPermission` → `includesPermission`

3. **Number Increment**: Parameters ending with numbers get incremented:
   - `item1` → `item2`
   - `value2` → `value3`

4. **Default Strategy**: Other parameters get prefixed with "alternative":
   - `userName` → `alternativeUserName`

## Usage

```bash
# Navigate to src directory
cd src

# Build the project
dotnet build

# Run the program
dotnet run <input_file.cs> <output_file.cs>

# Example with sample files
dotnet run samples/SampleInput.cs samples/SampleOutput.cs
```

## Example

**Input (`samples/SampleInput.cs`):**
```csharp
public int Square(int number)
{
    return number * number;
}

public string FormatText(string value)
{
    return value.ToUpper();
}
```

**Output:**
```csharp
public int Square(int number, int alternativeNumber)
{
    return number * number;
}

public string FormatText(string value, string newValue)
{
    return value.ToUpper();
}
```

## Dependencies

- .NET 9.0
- Microsoft.CodeAnalysis.CSharp 4.7.0
- Microsoft.CodeAnalysis.Common 4.7.0

## Project Structure

```
src/
├── Program.cs                    # Main Roslyn application
├── RoslynParameterDuplicator.csproj  # Project configuration
└── samples/                      # Sample files for testing
    ├── SampleInput.cs           # Input test file
    ├── SampleOutput.cs          # Expected output
    └── ExpectedOutput.cs.example # Reference example
```

## Notes

- Only methods with exactly one parameter are modified
- Methods with zero parameters or multiple parameters remain unchanged
- Original code structure and comments are preserved
- The program maintains proper C# syntax and formatting