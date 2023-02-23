using System.Reflection;

namespace Mapper.Prep;

public static class MapperConfigurationExtensions
{
  public static IMapper BuildSimpleMapperV2(this MappingConfigurationBuilder builder)
  {
    return new SimpleMapperV2(builder.Build(), new ICustomTypeMapper[] {
      new ArrayToListMapper(),
      new ListToArrayMapper()
    });
  }

  public static IMapper BuildExpressionMapper(this MappingConfigurationBuilder builder)
  {
    return new ExpressionMapper(builder.Build());
  }
  
  public static bool IsInitOnly(this PropertyInfo property)
  {
    if (!property.CanWrite)
    {
      return false;
    }
 
    var setMethod = property.SetMethod;
 
    // Get the modifiers applied to the return parameter.
    var setMethodReturnParameterModifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();
 
    // Init-only properties are marked with the IsExternalInit type.
    return setMethodReturnParameterModifiers.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
  }

  public static bool CanWrite(this MemberInfo memberInfo)
  {
    if (memberInfo.MemberType == MemberTypes.Field)
      return true;

    if (memberInfo.MemberType == MemberTypes.Property)
    {
      var prop = (PropertyInfo)memberInfo;
      return prop.CanWrite && !IsInitOnly(prop);
    }
    return false;
  }
}