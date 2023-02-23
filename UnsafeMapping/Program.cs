// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;

Console.WriteLine("Hello, World!");

A1 a = new A1() { X = 1000, Y = 10_000 };
ref B1 b = ref Map<A1, B1>(ref a);

b.X = 666;

Console.WriteLine(b.X);

var author = new Person() { Name = "Andrii", Surname = "Podkolzin" };
ref var result = ref Unsafe.As<Person, PersonDto>(ref author);
result.Name = "Ololo";

result.DoSmth();

Console.WriteLine(object.ReferenceEquals(result, author));
Console.WriteLine(result.GetType().FullName);



ref TDestination Map<TSource, TDestination>(ref TSource source) 
  where TSource : struct
  where  TDestination : struct
{
  return ref Unsafe.As<TSource, TDestination>(ref source);
}

struct A1
{
  public int X,
    Y;
}

struct B1
{
  public int X;
  private int _y;
}

class Person
{
  public string Name { get; set; }
  public string Surname { get; set; }

  //public void DoSmth() => Console.WriteLine("Person");
}

class PersonDto
{
  public string Name { get; set; }
  public string Surname { get; set; } 
  
  public string FullName { get; set; }
  
  public void DoSmth() => Console.WriteLine("Dto");
}