using System.Linq.Expressions;
using System.Reflection;

namespace Mapper.Prep;

// Model
internal record MappingConfiguration(IEnumerable<TypeMappingConfiguration> TypeConfigurations);

internal record TypeMappingConfiguration(Type From, Type To, IEnumerable<IMappingAction> MemberMappings);

internal interface IMappingAction
{
  public string Destination { get; }
}

internal record IgnoreMemberMappingAction(string Destination) : IMappingAction;

internal record MapMemberMappingAction(string Destination, Expression SourceExpression) : IMappingAction;

internal record MapAtRuntimeMappingAction(string Destination, Delegate SourceAction) : IMappingAction;

internal interface IMemberMappingConfigurationBuilder
{
  IMappingAction Build();
}

internal interface ITypeMappingConfigurationBuilder
{
  TypeMappingConfiguration Build();
}

public class MappingConfigurationBuilder
{
  private readonly List<ITypeMappingConfigurationBuilder> _memberMappings = new();
  
  public TypeMappingConfigurationBuilder<TFrom, TTo> CreateMap<TFrom, TTo>()
  {
    var item = new TypeMappingConfigurationBuilder<TFrom, TTo>();
    _memberMappings.Add(item);
    return item;
  }

  internal MappingConfiguration Build()
  {
    return new MappingConfiguration(_memberMappings.Select(x => x.Build()));
  }
}

public class TypeMappingConfigurationBuilder<TSource, TDest> : ITypeMappingConfigurationBuilder
{
  private readonly List<IMemberMappingConfigurationBuilder> _memberMappingConfigurations = new();
  
  public TypeMappingConfigurationBuilder<TSource, TDest> ForMember<TProp>(string destProperty, Action<MemberMappingConfigurationBuilder<TSource, TProp>> memberOptions)
  {
    var builder = new MemberMappingConfigurationBuilder<TSource, TProp>(destProperty);
    _memberMappingConfigurations.Add(builder);
    memberOptions(builder);
    return this;
  }
  
  public TypeMappingConfigurationBuilder<TSource, TDest> ForMember<TProp>(Expression<Func<TDest, TProp>> destProperty, Action<MemberMappingConfigurationBuilder<TSource, TProp>> memberOptions)
  {
    if (destProperty.Body is not MemberExpression memberExpression)
      throw new ArgumentException("Body should be member expression");
    if (memberExpression.Member.MemberType == MemberTypes.Property || memberExpression.Member.MemberType == MemberTypes.Field)
      return ForMember(memberExpression.Member.Name, memberOptions);

    throw new ArgumentException("Body should be Property or Field");
  }
  
  TypeMappingConfiguration ITypeMappingConfigurationBuilder.Build()
  {
    var config = new TypeMappingConfiguration(typeof(TSource), typeof(TDest), _memberMappingConfigurations.Select(x => x.Build()));
    return config;
  }
}

public class MemberMappingConfigurationBuilder<TSource, TMember> : IMemberMappingConfigurationBuilder
{
  private readonly string _destProperty;
  private bool _ignore;
  private Expression<Func<TSource, TMember>>? _sourceMember;
  private Func<TSource, TMember>? _mappingAction;
  
  public MemberMappingConfigurationBuilder(string destProperty)
  {
    _destProperty = destProperty;
  }

  public void MapFrom(Expression<Func<TSource, TMember>> sourceMember)
  {
    _ignore = false;
    _mappingAction = null;
    _sourceMember = sourceMember;
  }

  public void MapAtRuntime(Func<TSource, TMember> mappingAction)
  {
    _ignore = false;
    _mappingAction = mappingAction;
    _sourceMember = null;
  }

  public void Ignore()
  {
    _ignore = true;
  }

  IMappingAction IMemberMappingConfigurationBuilder.Build()
  {
    if (_ignore)
      return new IgnoreMemberMappingAction(_destProperty);
    else if (_sourceMember != null)
      return new MapMemberMappingAction(_destProperty, _sourceMember);
    else if (_mappingAction != null)
      return new MapAtRuntimeMappingAction(_destProperty, _mappingAction);
    throw new InvalidOperationException("Member mapping was not configured.");
  }
}