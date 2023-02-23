using System.Linq.Expressions;
using System.Reflection;

namespace Mapper.Prep;

public static class MemberInfoExtensions
{
  public static Type GetMemberType(this MemberInfo memberInfo)
  {
    return memberInfo.MemberType switch {
      MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
      MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
      _ => throw new ArgumentException("Invalid member type. Can't get member type")
    };
  }

  public static Expression BuildExpression(this MemberInfo memberInfo, Expression instanceExpression)
  {
    return memberInfo.MemberType switch {
      MemberTypes.Field => Expression.Field(instanceExpression, (FieldInfo)memberInfo),
      MemberTypes.Property => Expression.Property(instanceExpression, (PropertyInfo)memberInfo),
      _ => throw new ArgumentException("Invalid member type. Can't build expression")
    };
  }
}