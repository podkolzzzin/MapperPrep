using System.Reflection;

namespace Mapper.Prep;

internal interface ICustomTypeMapper
{
  bool CanMap(Type source, Type dest);
  object Map(SimpleMapperV2 mapper, Type dest, object sourceObj);
}

internal class ArrayToListMapper : ICustomTypeMapper
{
  public bool CanMap(Type source, Type dest)
  {
    return source.IsSZArray && (dest.IsGenericType && dest.GetGenericTypeDefinition() == typeof(List<>));
  }
  public object Map(SimpleMapperV2 mapper, Type dest, object sourceObj)
  {
    var method = GetType().GetMethod(nameof(MapGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!;
    return method.MakeGenericMethod(sourceObj.GetType().GetGenericArguments().Single(), dest.GetElementType()!)
      .Invoke(this, new[] { mapper, sourceObj })!;
  }

  private List<TDest> MapGeneric<TSource, TDest>(SimpleMapperV2 mapper, TSource[] items) 
    => items.Select(x => mapper.Map<TDest>(x)).ToList();
}

internal class ListToArrayMapper : ICustomTypeMapper
{
  public bool CanMap(Type source, Type dest)
  {
    // or it can be IList...
    return dest.IsSZArray && (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(List<>));
  }
  public object Map(SimpleMapperV2 mapper, Type dest, object sourceObj)
  {
    var method = GetType().GetMethod(nameof(MapGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!;
    return method.MakeGenericMethod(sourceObj.GetType().GetGenericArguments().Single(), dest.GetElementType()!)
      .Invoke(this, new[] { mapper, sourceObj })!;
  }

  private TDest[] MapGeneric<TSource, TDest>(SimpleMapperV2 mapper, List<TSource> items) 
    => items.Select(x => mapper.Map<TDest>(x)).ToArray();
}