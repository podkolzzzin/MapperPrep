using Mapper.Prep;

public static class MapperConfigurationExtensions
{
  public static SimpleMapperV2 BuildSimpleMapperV2(this MappingConfigurationBuilder builder)
  {
    return new SimpleMapperV2(builder.Build(), new ICustomTypeMapper[] {
      new ArrayToListMapper(),
      new ListToArrayMapper()
    });
  }
}