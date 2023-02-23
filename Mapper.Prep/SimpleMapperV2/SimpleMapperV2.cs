using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Security.Cryptography;
using Mapper.Prep;

public class SimpleMapperV2 : IMapper
{
  private readonly MappingConfiguration _configuration;
  private readonly IEnumerable<ICustomTypeMapper> _customTypeMappers;
  internal SimpleMapperV2(MappingConfiguration configuration, IEnumerable<ICustomTypeMapper> customTypeMappers)
  {
    _configuration = configuration;
    _customTypeMappers = customTypeMappers;
  }

  public TDest Map<TSource, TDest>(TSource source) => Map<TDest>(source!);

  public TDest Map<TDest>(object source) => (TDest)Map(typeof(TDest), source);

  private object Map(Type destType, object source)
  {
    if (destType == source.GetType()) // Or should we implement deep copy?..
      return source;
    
    var result = _configuration.TypeConfigurations.SingleOrDefault(x => x.From == source.GetType());
    if (result == null)
    {
      var customTypeMapper = _customTypeMappers.SingleOrDefault(x => x.CanMap(source.GetType(), destType));
      if (customTypeMapper != null)
      {
        return customTypeMapper.Map(this, destType, source);
      }
    }

    var sourceProps = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
    var destProps = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    var inst = CreateInstance(destType, sourceProps, source);

    foreach (var destProp in destProps)
    {
      if (!destProp.CanWrite || destProp.IsInitOnly())
        continue;
      
      var sourceProp = sourceProps.FirstOrDefault(x => x.Name == destProp.Name);
      object value = null!;
      var mapping = result.MemberMappings.FirstOrDefault(x => x.Destination == destProp.Name);
      if (mapping is IgnoreMemberMappingAction)
        continue;
      else if (mapping is MapAtRuntimeMappingAction memberMappingAction)
      {
        value = memberMappingAction.SourceAction.DynamicInvoke(source)!;
      }
      else if (sourceProp != null && sourceProp.CanRead && destProp.CanWrite)
      {
        value = sourceProp.GetValue(source)!;
      }
      if (destProp.PropertyType != value.GetType())
        value = Map(destProp.PropertyType, value);  
      destProp.SetValue(inst, value);
    }

    return inst;
  }

  private object? CreateInstance(Type destType, PropertyInfo[] sourceProperties, object source)
  {
    var ctors = destType.GetConstructors();
    var suitableCtor = ctors.First(x => x.GetParameters().All(arg => CanResolve(arg, sourceProperties)));
    var parameters = suitableCtor
      .GetParameters()
      .Select(x => Resolve(x, sourceProperties, source))
      .ToArray();
    return suitableCtor.Invoke(parameters);
  }

  private object? Resolve(ParameterInfo parameterInfo, PropertyInfo[] sourceProperties, object source)
  {
    if (parameterInfo.IsOptional)
      return null;
    
    var suitableByName = sourceProperties.FirstOrDefault(x => string.Compare(
      x.Name,
      parameterInfo.Name,
      StringComparison.OrdinalIgnoreCase) == 0);
    if (suitableByName != null && CanMap(suitableByName.PropertyType, parameterInfo.ParameterType))
      return Map(parameterInfo.ParameterType, suitableByName.GetValue(source)!);

    return false;
  }
  
  private bool CanResolve(ParameterInfo parameterInfo, PropertyInfo[] sourceProperties)
  {
    if (parameterInfo.IsOptional)
      return true;
    
    var suitableByName = sourceProperties.FirstOrDefault(x => string.Compare(
      x.Name,
      parameterInfo.Name,
      StringComparison.OrdinalIgnoreCase) == 0);
    if (suitableByName != null && CanMap(suitableByName.PropertyType, parameterInfo.ParameterType))
      return true;

    return false;
  }
  private bool CanMap(Type source, Type dest)
  {
    return true;
  }
}