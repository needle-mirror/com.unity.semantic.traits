using System.Collections.Generic;

interface ICodeGenerator<T>
{
    IEnumerable<string> Generate(string outputPath, T definition);
}
