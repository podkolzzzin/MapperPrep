using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Mapper.Prep;

public class ExpressionMapper : IMapper
{
  private record struct TypePair(Type Source, Type Destination);
  
  private interface IExecutionPlan
  {
    void Map(object source, object dest);
    object Map(object source);
  }
  
  private class ExecutionPlan<TSource, TDestination> : IExecutionPlan
  {
    private readonly ExpressionMapper _mapper;
    private readonly Type _source;
    private readonly Type _destination;

    private readonly MemberInfo[] _sourceMembers, _destinationMembers;

    private readonly ConstantExpression _mapperExpression;
    private readonly MethodInfo _mapMethod;

    private Expression<Action<TSource, TDestination>>? _updateObjectExpression;
    private Action<TSource, TDestination>? _updateObjectFunc;
    
    private Func<TSource, TDestination>? _mapObjectFunc;
    private Expression<Func<TSource, TDestination>>? _mapObjectExpression;

    public ExecutionPlan(ExpressionMapper mapper)
    {
      _mapper = mapper;
      _mapperExpression = Expression.Constant(mapper);
      _mapMethod = _mapper.GetType().GetMethod("Map", BindingFlags.Default, new []{ typeof(Type), typeof(object)})!;
      
      _source = typeof(TSource);
      _destination = typeof(TDestination);
      _sourceMembers = _source.GetMembers();
      _destinationMembers = _destination.GetMembers();
    }

    public void Map(object source, object dest)
    {
      if (_updateObjectFunc == null)
        BuildPlan();
      _updateObjectFunc!((TSource)source, (TDestination)dest);
    }

    public object Map(object source)
    {
      if (_mapObjectFunc == null)
        BuildPlan();
      return _mapObjectFunc!((TSource)source);
    }
    
    private void BuildPlan()
    {
      // TODO: inline
      var sourceExpression = Expression.Parameter(_source);
      var destinationExpression = Expression.Parameter(_destination, "destination");

      var newExpression = CreateCtorCallExpression(sourceExpression);
      var variable = Expression.Variable(_destination, "result");
      var assignmentExpression = Expression.Assign(variable, newExpression);
      var mapActions = new List<Expression>(_destinationMembers.Length + 2) {
        assignmentExpression
      };
      var updateActions = new List<Expression>(_destinationMembers.Length);
      
      CreateMappingActionExpressions(mapActions, variable, sourceExpression);
      mapActions.Add(variable);
      
      CreateMappingActionExpressions(updateActions, destinationExpression, sourceExpression);
      
      _updateObjectExpression = Expression.Lambda<Action<TSource, TDestination>>(
        Expression.Block(updateActions) , sourceExpression, destinationExpression);
      _updateObjectFunc = _updateObjectExpression.Compile();
      
      var block = Expression.Block(new[] { variable }, mapActions);
      _mapObjectExpression = Expression.Lambda<Func<TSource, TDestination>>(block, sourceExpression);
      _mapObjectFunc = _mapObjectExpression.Compile();
    }
    
    private void CreateMappingActionExpressions(List<Expression> actions, Expression destinationExpression, Expression sourceExpression)
    {
      foreach (var destination in _destinationMembers)
      {
        if (!destination.CanWrite())
          continue;
        
        var mappingAction = _mapper.Resolve(_source, _destination, destination.Name);
        if (mappingAction is IgnoreMemberMappingAction)
          continue;

        var prop = destination.BuildExpression(destinationExpression);
        actions.Add(Expression.Assign(prop, GetDestinationMemberExpression(destination.Name, mappingAction, destination.GetMemberType(), sourceExpression)));
      }
    }

    private NewExpression CreateCtorCallExpression(Expression sourceExpression)
    {
      var ctors = _destination.GetConstructors();
      var suitableCtor = ctors.First(x => x.GetParameters().All(arg => CanResolve(arg, _sourceMembers)));
      var parameters = suitableCtor.GetParameters();
      var argumentExpressions = new Expression[parameters.Length];
      
      for (int i = 0; i < parameters.Length; i++)
      {
        var param = parameters[i];
        if (param.IsOptional)
          argumentExpressions[i] = Expression.Constant(param.DefaultValue);
       
        var mappingAction = _mapper.Resolve(_source, _destination, param.Name!);
        if (mappingAction is IgnoreMemberMappingAction)
          throw new InvalidOperationException($"Can't ignore member that is required in ctor: {param.Name} {param.ParameterType.Name}");
        argumentExpressions[i] = GetDestinationMemberExpression(param.Name, mappingAction, param.ParameterType, sourceExpression);
      }
      return Expression.New(suitableCtor, argumentExpressions);
    }

    private Expression GetDestinationMemberExpression(string targetName, IMappingAction? mappingAction, Type targetType, Expression sourceExpression)
    {
      if (mappingAction != null)
      {
        if (mappingAction is IgnoreMemberMappingAction)
          throw new InvalidOperationException();
        else if (mappingAction is MapAtRuntimeMappingAction runtime)
        {
          var genericType = runtime.SourceAction.GetType();
          var func = Expression.Constant(runtime.SourceAction, genericType);
          return Expression.Call(func, genericType.GetMethod("Invoke")!, sourceExpression);
        }
        else if (mappingAction is MapMemberMappingAction mapMemberMappingAction)
        {
          return Expression.Invoke(mapMemberMappingAction.SourceExpression, sourceExpression);
        }
      }
      var member = Resolve(targetName, targetType);
      
      if (member.GetMemberType() != targetType)
        return Expression.Call(
          _mapperExpression,
          _mapMethod,
          Expression.Constant(targetType),
          member.BuildExpression(sourceExpression));
      else
      {
        return member.BuildExpression(sourceExpression);
      }
    }

    private MemberInfo Resolve(string name, Type type)
    {
      var suitableByName = _sourceMembers.FirstOrDefault(x => string.Compare(
        x.Name,
        name,
        StringComparison.OrdinalIgnoreCase) == 0);
      if (suitableByName != null && _mapper.CanMap(suitableByName.GetMemberType(), type))
        return suitableByName;

      throw new InvalidOperationException("Can't find suitable member to map");
    }
  
    private bool CanResolve(ParameterInfo parameterInfo, MemberInfo[] sourceProperties)
    {
      if (parameterInfo.IsOptional)
        return true;

      var suitableByName = sourceProperties.FirstOrDefault(x => string.Compare(
        x.Name,
        parameterInfo.Name,
        StringComparison.OrdinalIgnoreCase) == 0);
      if (suitableByName != null && _mapper.CanMap(suitableByName.GetMemberType(), parameterInfo.ParameterType))
        return true;

      return false;
    }
  }
  
  private readonly ImmutableDictionary<TypePair, TypeMappingConfiguration> _configuration;
  private readonly ConcurrentDictionary<TypePair, IExecutionPlan> _executionPlans = new();
  internal ExpressionMapper(MappingConfiguration configuration)
  {
    _configuration = configuration.TypeConfigurations.ToImmutableDictionary(x => new TypePair(x.From, x.To));
  }
  public TDest Map<TSource, TDest>(TSource source)
  {
    var pair = new TypePair(typeof(TSource), typeof(TDest));
    return (TDest)_executionPlans.GetOrAdd(pair, BuildExecutionPlan<TSource, TDest>).Map(source);
  }
  
  public TDest Map<TDest>(object source)
  {
    var pair = new TypePair(source.GetType(), typeof(TDest));
    return (TDest)_executionPlans.GetOrAdd(pair, BuildExecutionPlanNonGeneric).Map(source);
  }

  public object Map(Type destinationType, object source)
  {
    var pair = new TypePair(destinationType, source.GetType());
    return _executionPlans.GetOrAdd(pair, BuildExecutionPlanNonGeneric).Map(source);
  }
  
  private ExecutionPlan<TSource, TDest> BuildExecutionPlan<TSource, TDest>(TypePair pair)
   => new (this);

  private IMappingAction? Resolve(Type from, Type to, string memberName)
  {
    _configuration.TryGetValue(new(from, to), out var result);
    return result.MemberMappings.FirstOrDefault(x => x.Destination == memberName);
  }
  
  private IExecutionPlan BuildExecutionPlanNonGeneric(TypePair pair)
  {
    return (IExecutionPlan)GetType().GetMethod(nameof(BuildExecutionPlan), BindingFlags.NonPublic | BindingFlags.Instance)
      !.MakeGenericMethod(pair.Source, pair.Destination)
      .Invoke(this, new object[] { pair })!;
  }

  private bool CanMap(Type source, Type dest)
  {
    return true;
  }
}