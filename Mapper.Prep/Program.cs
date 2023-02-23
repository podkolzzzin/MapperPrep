// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using GeneratorBased;
using GeneratorBased.Runtime;
using Mapper.Prep;
using Mapster;

var type = typeof(Person[]);
Console.WriteLine(type.IsSZArray);

var author = new Person() {
  Name = "Andrii",
  Surname = "Podkolzin"
};

var mickJagger = new Person() { Name = "Mick", Surname = "Jagger" };
var band = new Band() {
  Name = "The Rolling Stones",
  FrontMan = mickJagger,
  Members = new List<Person>() {
    mickJagger,
    new() { Name = "Keith", Surname = "Richards" },
    new() { Name = "Ronnie", Surname = "Wood" },
    new() { Name = "Charlie", Surname = "Watts" }
  }
};



// builder.Map<Person, PersonDto>()
//   .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name));
//
// var result = SimpleMapperV1.Map<PersonDto>(author);
//
// Console.WriteLine(result.Name + " " + result.Surname);

var builder = new MappingConfigurationBuilder();
builder.CreateMap<Person, PersonDto>()
  .ForMember(x => x.FullName, x => x.MapAtRuntime(y => y.Name + " " + y.Surname));
builder.CreateMap<Band, BandDto>();

var mapper = builder.BuildSimpleMapperV2();
var result = mapper.Map<PersonDto>(author);
Console.WriteLine(result.Name + " " + result.Surname + "|" + result.FullName);

var bandDto = mapper.Map<BandDto>(band);
Console.WriteLine($"{bandDto.Name} with {bandDto.FrontMan.FullName} as a front man. Members: {string.Join(',', bandDto.Members.Select(x => x.FullName))}");

GeneratorMapperProfile p = new GeneratorMapperProfile();
// var r = p.PersonToPersonDtoMappingMap(author);
// Console.WriteLine(r.Name);


PersonDto Map(Person p)
{
  var dto = new PersonDto();
  dto.Name = p.Name;
  p.Surname = p.Surname;
  return dto;
}

class Band
{
  public string Name { get; set; }
  public Person FrontMan { get; set; }
  public IEnumerable<Person> Members { get; set; }
}

class BandDto
{
  public string Name { get; set; }
  public PersonDto FrontMan { get; set; }
  public PersonDto[] Members { get; set; }
}

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