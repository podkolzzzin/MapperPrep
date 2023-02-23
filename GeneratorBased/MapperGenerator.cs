using System.Collections.Immutable;
using System.Diagnostics;
using GeneratorBased.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RazorLight;

namespace GeneratorBased;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
  private static readonly Type ProfileType = typeof(MappingProfile);
  
  private static readonly RazorLightEngine RazorEngine =
    new RazorLightEngineBuilder()
      .UseFileSystemProject(Directory.GetCurrentDirectory())
      .UseMemoryCachingProvider()
      .Build();
  
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(x => x.AddSource("Test", "//Works 111!"));
    //if (!Debugger.IsAttached) { Debugger.Launch(); }
    var profiles = context.SyntaxProvider.CreateSyntaxProvider(
      (node, token) => node is ClassDeclarationSyntax,
      (ctx, token) =>
      {
        var node = (ClassDeclarationSyntax)ctx.Node;
        var typeInfo = (ITypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(node, token)!;
        if (typeInfo.BaseType.Name == "MappingProfile")
          return typeInfo;
        return null;
      }).Where(x => x != null);
    
    var compilationsAndClassDeclarations = context.CompilationProvider.Combine(profiles.Collect());
    
    context.RegisterSourceOutput(compilationsAndClassDeclarations,
      static (spc, source) => Execute(source.Left, source.Right, spc));
  }
  private static void Execute(Compilation sourceLeft, ImmutableArray<ITypeSymbol?> classDeclarations, SourceProductionContext context)
  {
    context.AddSource("Test2", "//Works too!");
    if (classDeclarations.IsDefaultOrEmpty)
    {
      return;
    }
    context.AddSource("Test3", "//Works too!");
    var distinctClassDeclarations = classDeclarations.Distinct(SymbolEqualityComparer.Default).ToArray();

    foreach (var profile in distinctClassDeclarations)
    {
      context.AddSource(profile.Name + ".g.cs", BuildSource((ITypeSymbol)profile, context));
    }
  }
  private static string BuildSource(ITypeSymbol profile, SourceProductionContext context)
  {
    var members = profile.GetMembers()
      .OfType<IPropertySymbol>()
      .Where(x => x.Type.Name == "Mapping");

    var methods = new List<MapperTemplate.MethodModel>();
    foreach (var member in members)
    {
      var t = (INamedTypeSymbol)member.Type;
      var sourceType = t.TypeArguments[0];
      var destType = t.TypeArguments[1];
      
      methods.Add(new MapperTemplate.MethodModel() {
        SourceType = sourceType.Name, 
        DestinationType = destType.Name, 
        MethodName = member.Name + "Map",
        Mappings = BuildMappings(sourceType, destType)
      });
      
    }
    var template = new MapperTemplate() {
      Namespace = profile.ContainingNamespace.Name,
      ClassName = profile.Name,
      Methods = methods
    };
    var text = template.TransformText();
    return text;
  }
  private static IEnumerable<MapperTemplate.Mapping> BuildMappings(ITypeSymbol sourceType, ITypeSymbol destType)
  {
    var destMembers = destType.GetMembers()
      .OfType<IPropertySymbol>();
    var sourceMembers = sourceType.GetMembers()
      .OfType<IPropertySymbol>();
    
    foreach (var destProp in destMembers)
    {
      var sourceMember = sourceMembers.FirstOrDefault(x => x.Name == destProp.Name);
      if (sourceMember != null)
      {
        yield return new MapperTemplate.Mapping() {
          From = sourceMember.Name,
          To = destProp.Name
        };
      }
    }
  }
}