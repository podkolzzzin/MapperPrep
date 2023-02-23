using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using GeneratorBased;
using GeneratorBased.Runtime;

namespace GeneratorTests.Tests
{
  public class GeneratorTests
    {
        [Fact]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation("""
using GeneratorBased.Runtime;

namespace MyCode
{
  class Person
  {
    public string Name { get; set; }
    public string Surname { get; set; }
  }

  class PersonDto
  {
    public string Name { get; set; }
    public string Surname { get; set; } 
    
    public string FullName { get; set; }
  }

  partial class GeneratorMapperProfile : MappingProfile
  {
    public Mapping<Person, PersonDto> PersonToPersonDtoMapping => new();
  }
}
""");
            var generator = new MapperGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] {
                  MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                  MetadataReference.CreateFromFile(typeof(MappingProfile).GetTypeInfo().Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication
                ));
    }
}