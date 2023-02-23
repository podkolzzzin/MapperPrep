// See https://aka.ms/new-console-template for more information

using GeneratorBased.Runtime;
using MyCode;
Console.WriteLine("Hello");

Console.WriteLine("Hello");
var dto = new GeneratorMapperProfile().PersonToPersonDtoMappingMap(new Person() {
  Name = "Hello",
  Surname = "World"
});
Console.WriteLine(dto.Name);

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