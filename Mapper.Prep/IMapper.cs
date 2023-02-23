
public interface IMapper
{
  TDest Map<TSource, TDest>(TSource source);
  TDest Map<TDest>(object source);
}