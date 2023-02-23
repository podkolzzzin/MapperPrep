using System.Reflection;

static class SimpleMapperV1
{
  public static TDest Map<TDest>(object source) where TDest : new()
  {
    var sourceProps = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
    var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    var inst = new TDest();

    foreach (var destProp in destProps)
    {
      var sourceProp = sourceProps.FirstOrDefault(x => x.Name == destProp.Name);
      if (sourceProp != null && sourceProp.CanRead && destProp.CanWrite)
        destProp.SetValue(inst, sourceProp.GetValue(source));
    }

    return inst;
  }
}