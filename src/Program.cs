using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynParameterDuplicator;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: RoslynParameterDuplicator <input_file.cs> <output_file.cs>");
            Console.WriteLine("Example: RoslynParameterDuplicator input.cs output.cs");
            return;
        }

        string inputFilePath = args[0];
        string outputFilePath = args[1];

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Error: Input file '{inputFilePath}' not found.");
            return;
        }

        try
        {
            var processor = new ParameterDuplicator();
            await processor.ProcessFileAsync(inputFilePath, outputFilePath);
            Console.WriteLine(
                $"Successfully processed '{inputFilePath}' and saved result to '{outputFilePath}'"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file: {ex.Message}");
        }
    }
}

public class ParameterDuplicator
{
    public async Task ProcessFileAsync(string inputFilePath, string outputFilePath)
    {
        // Read the source code and get the C# file as a string
        // sourceCode: Contains the entire C# file content as text (comments, whitespace, everything)
        string sourceCode = await File.ReadAllTextAsync(inputFilePath);

        // Parse the source code into a syntax tree
        // SyntaxTree: Immutable tree structure representing the parsed C# code
        // ├── Root: CompilationUnitSyntax (entire file)
        // ├── Tokens: Keywords, identifiers, operators, literals
        // └── Trivia: Whitespace, comments, formatting
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Get the root node of the syntax tree
        // CompilationUnitSyntax: Top-level node representing entire C# file
        // ├── Usings: using directives (using System;)
        // ├── Members: Namespaces, classes, types declared at file level
        // ├── AttributeLists: Assembly-level attributes
        // └── Externs: Extern alias directives
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

        // Transform the syntax tree (modificates the node and create new ones if the functions have a single parameter)
        // ParameterDuplicationRewriter: Custom CSharpSyntaxRewriter that implements Visitor pattern
        // ├── Inherits: CSharpSyntaxRewriter (provides base visiting functionality)
        // ├── Overrides: VisitMethodDeclaration (called for each method in the tree)
        // └── State: ProcessedMethodsCount (tracks how many methods were modified)
        var rewriter = new ParameterDuplicationRewriter();

        /// DFS through all nodes and adding the requiered modifications
        // SyntaxNode: Result of the visitor pattern traversal
        // ├── Input: Original CompilationUnitSyntax root
        // ├── Process: Depth-first search through all nodes, calling appropriate Visit methods
        // ├── Modification: Only MethodDeclarationSyntax nodes with 1 parameter are changed
        // └── Output: New immutable syntax tree with duplicated parameters
        SyntaxNode newRoot = rewriter.Visit(root);

        // Transform the syntax tree back into string
        // ToFullString(): Converts syntax tree back to C# source code text
        // ├── Fidelity: Preserves ALL original formatting, whitespace, comments
        // ├── Changes: Only modified nodes (duplicated parameters) are different
        // └── Output: Complete C# source code as string ready for file writing
        string modifiedCode = newRoot.ToFullString();

        // Write the modified source code to output file
        // File.WriteAllTextAsync(): Asynchronously writes string to file
        // ├── Async: Doesn't block thread during I/O operation
        // ├── UTF-8: Uses UTF-8 encoding by default
        // ├── Overwrite: Creates new file or overwrites existing
        // └── Exception: Can throw IOException, UnauthorizedAccessException, etc.
        await File.WriteAllTextAsync(outputFilePath, modifiedCode);

        Console.WriteLine(
            $"Found and processed {rewriter.ProcessedMethodsCount} method(s) with single parameters."
        );
    }
}

public class ParameterDuplicationRewriter : CSharpSyntaxRewriter
{
    public int ProcessedMethodsCount { get; private set; }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Check if method has exactly one parameter
        // ParameterList: Contains the parentheses and all parameters
        // ├── Parameters: SeparatedSyntaxList<ParameterSyntax> collection
        // ├── Count: Number of parameters in the method
        // └── Condition: Only process methods with exactly 1 parameter
        if (node.ParameterList.Parameters.Count == 1)
        {
            // Get the single parameter from the method
            // Parameters[0]: First (and only) parameter in the collection
            // ParameterSyntax: Represents complete parameter definition
            // ├── Type: Parameter type (int, string, etc.)
            // ├── Identifier: Parameter name (number, value, etc.)
            // ├── Modifiers: ref, out, params, in modifiers
            // ├── Default: Default value if any (= 5)
            // └── AttributeLists: Parameter attributes if any
            var originalParameter = node.ParameterList.Parameters[0];

            // Create a duplicate parameter with a suggested name
            // CreateDuplicateParameter(): Creates new ParameterSyntax with different name
            // ├── Input: Original ParameterSyntax (preserves type, modifiers)
            // ├── Process: Generate suggested name using naming strategies
            // ├── Output: New ParameterSyntax with same type but different identifier
            // └── Example: "int number" → "int alternativeNumber"
            var duplicateParameter = CreateDuplicateParameter(originalParameter);

            // Create new parameter list with both parameters
            // SyntaxFactory.SeparatedList(): Creates collection with comma separators
            // ├── Input: Array of ParameterSyntax objects
            // ├── Ordering: [originalParameter, duplicateParameter]
            // ├── Separators: Commas automatically added between parameters
            // └── Output: SeparatedSyntaxList<ParameterSyntax> for parameter list
            var newParameterList = node.ParameterList.WithParameters(
                SyntaxFactory.SeparatedList(new[] { originalParameter, duplicateParameter })
            );

            // Update the method with new parameter list
            // WithParameterList(): Creates new MethodDeclarationSyntax with modified parameters
            // ├── Immutable: Original method node is unchanged
            // ├── Preserves: Return type, method name, body, modifiers, attributes
            // ├── Changes: Only the parameter list is replaced
            // └── Result: New method node with duplicated parameters
            var newMethod = node.WithParameterList(newParameterList);

            // Increment counter for processed methods
            // ProcessedMethodsCount: Tracks how many methods were successfully modified
            // ├── Purpose: Provides statistics for user feedback
            // ├── Scope: Instance variable, persists across all method visits
            // └── Usage: Displayed in final console output message
            ProcessedMethodsCount++;

            // Log the transformation for debugging/user feedback
            // Console.WriteLine(): Immediate feedback showing what was changed
            // ├── originalParameter.Identifier.ValueText: Gets parameter name as string
            // ├── duplicateParameter.Identifier.ValueText: Gets new parameter name
            // ├── node.Identifier.ValueText: Gets method name as string
            // └── Purpose: User can see exactly what modifications were made
            Console.WriteLine(
                $"Duplicated parameter '{originalParameter.Identifier.ValueText}' "
                    + $"as '{duplicateParameter.Identifier.ValueText}' "
                    + $"in method '{node.Identifier.ValueText}'"
            );

            // Return the modified method node
            // newMethod: New MethodDeclarationSyntax with duplicated parameters
            // ├── Replaces: Original method node in the syntax tree
            // ├── Visitor: Roslyn automatically incorporates this into new tree
            // └── Result: Method now has 2 parameters instead of 1
            return newMethod;
        }

        // Return original method unchanged (not single parameter)
        // base.VisitMethodDeclaration(): Calls parent class implementation
        // ├── Purpose: Continue normal visitor pattern traversal
        // ├── Result: Returns original node without modifications
        // ├── Cases: Methods with 0, 2, 3+ parameters reach here
        // └── Visitor: Continues processing other nodes in the method
        return base.VisitMethodDeclaration(node);
    }

    private ParameterSyntax CreateDuplicateParameter(ParameterSyntax originalParameter)
    {
        // Extract the parameter name as string
        // Identifier.ValueText: Gets the actual parameter name without quotes or syntax tokens
        // ├── Input: SyntaxToken containing parameter identifier
        // ├── Output: Clean string representation (e.g., "number", "value", "data")
        // └── Purpose: Used for generating suggested alternative name
        string originalName = originalParameter.Identifier.ValueText;

        // Generate a suggested alternative name
        // GenerateSuggestedParameterName(): Applies naming strategies to create meaningful alternatives
        // ├── Strategies: Dictionary lookup, number increment, prefix/suffix rules
        // ├── Input: Original parameter name string
        // ├── Output: Suggested alternative name string
        // └── Examples: "number" → "alternativeNumber", "item1" → "item2", "value" → "newValue"
        string suggestedName = GenerateSuggestedParameterName(originalName);

        // Create new parameter with suggested name but same type and modifiers
        // WithIdentifier(): Creates new ParameterSyntax with different identifier
        // ├── Preserves: Type, modifiers (ref/out), default values, attributes
        // ├── Changes: Only the parameter identifier/name
        // ├── SyntaxFactory.Identifier(): Creates new identifier token with suggested name
        // ├── WithLeadingTrivia(): Adds space before the parameter for proper formatting
        // └── Result: New ParameterSyntax identical to original except for name
        return originalParameter
            .WithIdentifier(SyntaxFactory.Identifier(suggestedName))
            .WithLeadingTrivia(SyntaxFactory.Space);
    }

    private string GenerateSuggestedParameterName(string originalName)
    {
        // Strategy for generating suggested names:
        // 1. If name ends with a number, increment it
        // 2. If name is a common pattern, suggest semantic alternative
        // 3. Otherwise, append "2" or "Alt"

        var suggestions = new Dictionary<string, string>
        {
            // Common parameter patterns and their suggested alternatives
            { "value", "newValue" },
            { "item", "otherItem" },
            { "data", "additionalData" },
            { "input", "secondInput" },
            { "output", "secondOutput" },
            { "source", "destination" },
            { "text", "otherText" },
            { "name", "displayName" },
            { "id", "secondId" },
            { "key", "secondaryKey" },
            { "count", "maxCount" },
            { "size", "preferredSize" },
            { "index", "startIndex" },
            { "length", "maxLength" },
        };

        // Check for direct matches in our suggestion dictionary
        if (suggestions.ContainsKey(originalName.ToLower()))
        {
            return suggestions[originalName.ToLower()];
        }

        // Check if name ends with a number and increment it
        if (originalName.Length > 0 && char.IsDigit(originalName[^1]))
        {
            string baseName = originalName.TrimEnd(
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            );
            string numberPart = originalName.Substring(baseName.Length);
            if (int.TryParse(numberPart, out int number))
            {
                return baseName + (number + 1);
            }
        }

        // Check for common prefixes and suggest alternatives
        if (originalName.StartsWith("is", StringComparison.OrdinalIgnoreCase))
        {
            return "shouldBe" + originalName.Substring(2);
        }

        if (originalName.StartsWith("has", StringComparison.OrdinalIgnoreCase))
        {
            return "includes" + originalName.Substring(3);
        }

        // Default strategies
        if (originalName.Length <= 3)
        {
            return originalName + "2";
        }

        return "alternative" + char.ToUpper(originalName[0]) + originalName.Substring(1);
    }
}

/*
ROSLYN SYNTAX TREE AND NODE CONCEPTS:

SyntaxTree: Immutable tree representation of C# source code
├── Purpose: Parses source code text into structured, navigable tree
├── Root: Always CompilationUnitSyntax (represents entire .cs file)
├── Nodes: Hierarchical structure of all language constructs
├── Tokens: Individual keywords, identifiers, operators, literals
├── Trivia: Whitespace, comments, formatting (preserved for fidelity)
├── Immutable: Cannot be modified directly, must create new trees
└── Navigation: Parent/child relationships, descendant traversal

SyntaxNode: Base class for all nodes in the syntax tree
├── Hierarchy: Abstract base class with concrete implementations
│   ├── CompilationUnitSyntax: Entire file (usings, namespaces, types)
│   ├── NamespaceDeclarationSyntax: namespace declarations
│   ├── ClassDeclarationSyntax: class declarations
│   ├── MethodDeclarationSyntax: method declarations ← Used in this program
│   ├── ParameterSyntax: method parameters ← Used in this program
│   ├── PropertyDeclarationSyntax: property declarations
│   ├── IfStatementSyntax: if statements
│   └── ... (100+ different syntax node types)
├── Properties: Each node type has specific properties for its construct
├── Methods: WithXxx() methods for creating modified copies
├── Visitor Pattern: CSharpSyntaxRewriter enables tree transformations
└── Immutability: Modifications create new nodes, original unchanged

VISITOR PATTERN IN ROSLYN:
├── CSharpSyntaxRewriter: Base class for syntax tree transformations
├── Visit Methods: VisitXxx() called automatically for each node type
├── Depth-First: Traverses entire tree visiting all nodes recursively
├── Selective Modification: Override specific VisitXxx() methods to modify certain nodes
├── Tree Reconstruction: Automatically builds new tree with modifications
└── Immutable Results: Original tree unchanged, new tree with modifications returned

MethodDeclarationSyntax: Specific SyntaxNode representing complete method declarations
├── Inheritance: MethodDeclarationSyntax : SyntaxNode (specialized syntax node)
├── Represents: Complete method definition including signature and body
├── Components:
│   ├── AttributeLists: Method attributes like [Obsolete], [JsonIgnore]
│   ├── Modifiers: public, private, static, virtual, override, async, etc.
│   ├── ReturnType: void, int, string, Task<T>, custom types
│   ├── Identifier: Method name (Square, FormatText, ProcessData)
│   ├── TypeParameterList: Generic parameters <T>, <TKey, TValue>
│   ├── ParameterList: (int number, string value) - parentheses and parameters
│   ├── ConstraintClauses: Generic constraints like where T : class
│   ├── Body: { ... } - method implementation in braces
│   ├── ExpressionBody: => expression - for expression-bodied methods
│   └── SemicolonToken: ; - for abstract methods or interface declarations
├── Examples:
│   ├── Simple: public int Square(int number) { return number * number; }
│   ├── Static: public static void ProcessData(byte[] data) { ... }
│   ├── Generic: public T Transform<T>(T item) where T : class { return item; }
│   ├── Async: public async Task<string> ReadFileAsync() { ... }
│   └── Expression: public int Double(int x) => x * 2;
├── Access Patterns:
│   ├── node.Identifier.ValueText - Gets method name as string
│   ├── node.ParameterList.Parameters - Gets collection of parameters
│   ├── node.ReturnType - Gets return type syntax
│   ├── node.Modifiers - Gets modifiers like public, static
│   └── node.Body - Gets method body statements
└── Modification: Use WithXxx() methods to create modified copies
    ├── WithParameterList() - Change parameters (used in this program)
    ├── WithModifiers() - Change access modifiers
    ├── WithReturnType() - Change return type
    └── WithBody() - Change method implementation
*/
