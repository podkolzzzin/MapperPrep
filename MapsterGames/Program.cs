// See https://aka.ms/new-console-template for more information

using System;
using System.Reflection;
using Mapster;

Console.WriteLine("Hello");

public class MyRegister : ICodeGenerationRegister
{
  public void Register(CodeGenerationConfig config)
  {
    config.AdaptTo("[name]Dto")
      .ForAllTypesInNamespace(Assembly.GetExecutingAssembly(), "Sample.CodeGen.Domains");

    config.GenerateMapper("[name]Mapper")
      .ForType<Course>()
      .ForType<Student>();
  }
}

public record Student(string Name, string Surname);
public record Course(string Name, string Professor, int LectionCount, IEnumerable<Student> Students);