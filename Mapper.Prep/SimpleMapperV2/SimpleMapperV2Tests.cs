using Mapper.Prep;
using Xunit;

public class SimpleMapperV2Tests
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
      .ForMember(x => x.FullName, x => x.MapAtRuntime(y => y.Name + " " + y.Surname));

    var mapper = builder.BuildSimpleMapperV2();
    var result = mapper.Map<PersonDto>(author);

    Assert.Equal(author.Name, result.Name);
    Assert.Equal(author.Surname, result.Surname);
    Assert.Equal("Andrii Podkolzin", result.FullName);
  }

  [Fact]
  public void CollectionMap()
  {
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
    var builder = new MappingConfigurationBuilder();
    builder.CreateMap<Person, PersonDto>()
      .ForMember(x => x.FullName, x => x.MapAtRuntime(y => y.Name + " " + y.Surname));
    builder.CreateMap<Band, BandDto>();

    var mapper = builder.BuildSimpleMapperV2();
    var bandDto = mapper.Map<BandDto>(band);
    
    Assert.Equal(mickJagger.Name, bandDto.FrontMan.Name);
    Assert.Equal(mickJagger.Surname, bandDto.FrontMan.Surname);
    Assert.NotNull(bandDto.FrontMan.FullName);
    
    Assert.Collection(bandDto.Members, el1 =>
    {
      Assert.Equal("Mick", el1.Name);
    }, el2 =>
    {
      Assert.NotNull(el2.FullName);
    }, el3 =>
    {
      Assert.Equal("Wood", el3.Surname);
    }, el4 =>
    {
      Assert.Equal("Charlie Watts", el4.FullName);
    });
    
    // If Preserve References
    //Assert.Equal(bandDto.FrontMan, bandDto.Members.First());
  }

  public class Book
  {
    public string Author { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
  }

  public record BookDto(string Author, string Name, string Text, int PageCount = 0);
  
  [Fact]
  public void SimpleRecordMap()
  {
    var book = new Book() { Author = "Terry Pratchett", Name = "Guards! Guards!", Text = "Sir Samuel..." };
    var builder = new MappingConfigurationBuilder();
    builder.CreateMap<Book, BookDto>();
    var mapper = builder.BuildSimpleMapperV2();
    var dto = mapper.Map<BookDto>(book);
    Assert.Equal(book.Author, dto.Author);
    Assert.Equal(book.Name, dto.Name);
    Assert.Equal(book.Text, dto.Text);
  }
}