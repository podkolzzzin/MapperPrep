using Xunit;

namespace Mapper.Prep;

public class ExpressionMapperTests
{
  [Fact]
  public void SimpleMap()
  {
    var author = new Person() {
      Name = "Andrii",
      Surname = "Podkolzin"
    };
    var builder = new MappingConfigurationBuilder();

    builder.CreateMap<Person, PersonDto>()
      .ForMember(x => x.FullName, x => x.MapFrom(y => y.Name + " " + y.Surname));

    var mapper = builder.BuildExpressionMapper();
    var result = mapper.Map<PersonDto>(author);

    Assert.Equal(author.Name, result.Name);
    Assert.Equal(author.Surname, result.Surname);
    Assert.Equal("Andrii Podkolzin", result.FullName);
  }
  
  [Fact]
  public void SimpleMapAtRuntime()
  {
    var author = new Person() {
      Name = "Andrii",
      Surname = "Podkolzin"
    };
    var builder = new MappingConfigurationBuilder();

    builder.CreateMap<Person, PersonDto>()
      .ForMember(x => x.FullName, x => x.MapAtRuntime(y => y.Name + " " + y.Surname));

    var mapper = builder.BuildExpressionMapper();
    var result = mapper.Map<PersonDto>(author);

    Assert.Equal(author.Name, result.Name);
    Assert.Equal(author.Surname, result.Surname);
    Assert.Equal("Andrii Podkolzin", result.FullName);
  }
}