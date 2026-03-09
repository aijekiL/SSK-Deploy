using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Venus.Service.EfCore;
using Venus.Tools.Enums;
using Venus.Tools.Extension;
using Venus.Tools.Request;
using Venus.Tools.Response;
using Venus.Tools;

namespace Venus.Service.EfCore
{
    /// <summary>
    /// Lambad表达式扩展
    /// </summary>
    public static class LambdaExtensions
    {

        /// <summary>
        /// 获取处理后的页数据
        /// </summary>
        /// <param name="options"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static PageGridData<TEntity> GetPageDataOptions<TEntity>(this IQueryable<TEntity> queryable, PageDataOptions options)
        {
            PageGridData<TEntity> pageGridData = new PageGridData<TEntity>();

            options = options ?? new PageDataOptions();
            var Properties = typeof(TEntity).GetProperties();

            #region 生成搜索条件
            List<SearchParameters> searchParametersList = new List<SearchParameters>();
            if (options.Filter != null && options.Filter.Count > 0)
            {
                searchParametersList.AddRange(options.Filter);
            }

            //  Connection
            // queryable = repository.DbContext.Set<T>();
            //2020.08.15添加自定义原生查询sql或多租户


            //判断列的数据类型数字，日期的需要判断值的格式是否正确
            for (int i = 0; i < searchParametersList.Count; i++)
            {
                SearchParameters x = searchParametersList[i];
                x.DisplayType = x.DisplayType.GetDBCondition();
                if (string.IsNullOrEmpty(x.Value))
                {
                    continue;
                }
                PropertyInfo property = Properties.Where(c => c.Name.ToUpper() == x.Name.ToUpper()).FirstOrDefault();
                //2020.06.25增加字段null处理
                if (property == null) continue;
                // property
                //移除查询的值与数据库类型不匹配的数据
                //object[] values = property.ValidationValueForDbType(x.Value.Split(',')).Where(q => q.Item1).Select(s => s.Item3).ToArray();
                //if (values == null || values.Length == 0)
                //{
                //    continue;
                //}

                //移除扩展字段的查询方法，扩展字段需要业务自己过滤
                if (property.IsDefined(typeof(NotMappedAttribute), false))
                {
                    continue;
                }

                if (x.DisplayType == StringExtension.HtmlElementType.Contains)
                    x.Value = string.Join(",", x.Value);
                LinqExpressionType expressionType = x.DisplayType.GetLinqCondition();

                if (expressionType == LinqExpressionType.In)
                    new Exception("未实现In语句查询");

                //var ss= LambdaExtensions.CreateExpression<Ly_UserLottery>("", "", expressionType);

                queryable = LinqExpressionType.In == expressionType
                              ? queryable.Where(x.Name.CreateExpression<TEntity>(x.Value, expressionType))
                              : queryable.Where(x.Name.CreateExpression<TEntity>(x.Value, expressionType));
            }

            #endregion

            #region 生成排序规则

            Dictionary<string, QueryOrderBy> sortDic = new Dictionary<string, QueryOrderBy>();

            if (!string.IsNullOrEmpty(options.sort))
            {
                if (options.sort.Contains(','))
                {
                    var sortArr = options.sort.Split(',');
                    foreach (var name in sortArr)
                    {
                        //存在空格，代表有指定排序规则
                        if (name.Contains(' '))
                        {
                            var nameArr = name.Trim().Split(' ');
                            if (nameArr.Length == 2)
                            {
                                var n = Properties.FirstOrDefault(x => x.Name.ToLower() == nameArr[0].ToLower())?.Name ?? null;
                                if (n != null)
                                {
                                    sortDic.Add(n, nameArr[1].ToLower() == "asc" ? QueryOrderBy.Asc : QueryOrderBy.Desc);
                                }
                            }
                        }
                        else
                        {
                            var n = Properties.FirstOrDefault(x => x.Name.ToLower() == name.Trim().ToLower())?.Name ?? null;
                            if (n != null)
                            {
                                sortDic.Add(n, options.Order?.ToLower() == "asc" ? QueryOrderBy.Asc : QueryOrderBy.Desc);
                            }
                        }
                    }
                }
                else if (Properties.Any(x => x.Name.ToLower() == options.sort.ToLower()))
                {
                    sortDic = new Dictionary<string, QueryOrderBy>() { {
                            options.sort,
                            options.Order?.ToLower() == "asc"? QueryOrderBy.Asc: QueryOrderBy.Desc
                     } };
                }
            }
            else
            {
                // 添加默认排序规则，避免 EF Core 警告
                // 尝试查找常见的 ID 字段作为默认排序
                var idProperty = Properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                                                               p.Name.Equals("ObjectId", StringComparison.OrdinalIgnoreCase) ||
                                                               p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                if (idProperty != null)
                {
                    sortDic = new Dictionary<string, QueryOrderBy>() { { idProperty.Name, QueryOrderBy.Desc } };
                }
                else
                {
                    // 如果找不到 ID 字段，使用第一个属性进行排序
                    var firstProperty = Properties.FirstOrDefault();
                    if (firstProperty != null)
                    {
                        sortDic = new Dictionary<string, QueryOrderBy>() { { firstProperty.Name, QueryOrderBy.Desc } };
                    }
                }
            }

            if (sortDic != null && sortDic.Count > 0)
            {
                if (sortDic != null && sortDic.Count > 0)
                {
                    queryable = queryable.GetIQueryableOrderBy(sortDic);
                }
            }
            #endregion

            //数据总数量
            pageGridData.total = queryable.Count();
            //每页大小大于0，则执行分页存在
            if (options.pageSize > 0)
            {
                if (options.Page <= 0)
                    options.Page = 1;
                pageGridData.rows = queryable.Skip((options.Page - 1) * options.pageSize).Take(options.pageSize).ToList();
            }
            else
            {
                pageGridData.rows = queryable.ToList();
            }
            pageGridData.page = options.Page;
            pageGridData.pageSize = options.pageSize;
            return pageGridData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">字段名</param>
        /// <param name="propertyValue">表达式的值</param>
        /// <param name="expressionType">创建表达式的类型,如:p=>p.propertyName != propertyValue 
        /// p=>p.propertyName.Contains(propertyValue)</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> CreateExpression<T>(this string propertyName, object propertyValue, LinqExpressionType expressionType)
        {
            return propertyName.CreateExpression<T>(propertyValue, null, expressionType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">字段名</param>
        /// <param name="propertyValue">表达式的值</param>
        /// <param name="expressionType">创建表达式的类型,如:p=>p.propertyName != propertyValue 
        /// p=>p.propertyName.Contains(propertyValue)</param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> CreateExpression<T>(
          this string propertyName,
          object propertyValue,
          ParameterExpression parameter,
          LinqExpressionType expressionType)
        {
            Type proType = typeof(T).GetProperties().FirstOrDefault(o => o.Name.ToLower() == propertyName.ToLower()).PropertyType;
            //创建节点变量如p=>的节点p
            //  parameter ??= Expression.Parameter(typeof(T), "p");//创建参数p
            parameter = parameter ?? Expression.Parameter(typeof(T), "p");

            //创建节点的属性p=>p.name 属性name
            MemberExpression memberProperty = Expression.PropertyOrField(parameter, propertyName);
            if (expressionType == LinqExpressionType.In)
            {
                if (!(propertyValue is System.Collections.IList list) || list.Count == 0) throw new Exception("属性值类型不正确");

                bool isStringValue = true;
                List<object> objList = new List<object>();

                if (proType.ToString() != "System.String")
                {
                    isStringValue = false;
                    foreach (var value in list)
                    {
                        objList.Add(value.ToString().ChangeType(proType));
                    }
                    list = objList;
                }

                if (isStringValue)
                {
                    //string 类型的字段，如果值带有'单引号,EF会默认变成''两个单引号
                    MethodInfo method = typeof(System.Collections.IList).GetMethod("Contains");
                    //创建集合常量并设置为常量的值
                    ConstantExpression constantCollection = Expression.Constant(list);
                    //创建一个表示调用带参数的方法的：new string[]{"1","a"}.Contains("a");
                    MethodCallExpression methodCall = Expression.Call(constantCollection, method, memberProperty);
                    return Expression.Lambda<Func<T, bool>>(methodCall, parameter);
                }
                //非string字段，按上面方式处理报异常Null TypeMapping in Sql Tree
                BinaryExpression body = null;
                foreach (var value in list)
                {
                    ConstantExpression constantExpression = Expression.Constant(value);
                    UnaryExpression unaryExpression = Expression.Convert(memberProperty, constantExpression.Type);

                    body = body == null
                        ? Expression.Equal(unaryExpression, constantExpression)
                        : Expression.OrElse(body, Expression.Equal(unaryExpression, constantExpression));
                }
                return Expression.Lambda<Func<T, bool>>(body, parameter);
            }

            //  object value = propertyValue;
            ConstantExpression constant = proType.ToString() == "System.String"
                ? Expression.Constant(propertyValue) : Expression.Constant(propertyValue.ToString().ChangeType(proType));

            // DateTime只选择了日期的时候自动在结束日期加一天，修复DateTime类型使用日期区间查询无法查询到结束日期的问题
            if ((proType == typeof(DateTime) || proType == typeof(DateTime?)) && expressionType == LinqExpressionType.LessThanOrEqual && propertyValue.ToString().Length == 10)
            {
                constant = Expression.Constant(Convert.ToDateTime(propertyValue.ToString()).AddDays(1));
            }

            UnaryExpression member = Expression.Convert(memberProperty, constant.Type);
            Expression<Func<T, bool>> expression;
            switch (expressionType)
            {
                //p=>p.propertyName == propertyValue
                case LinqExpressionType.Equal:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.Equal(member, constant), parameter);
                    break;
                //p=>p.propertyName != propertyValue
                case LinqExpressionType.NotEqual:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.NotEqual(member, constant), parameter);
                    break;
                //   p => p.propertyName > propertyValue
                case LinqExpressionType.GreaterThan:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(member, constant), parameter);
                    break;
                //   p => p.propertyName < propertyValue
                case LinqExpressionType.LessThan:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.LessThan(member, constant), parameter);
                    break;
                // p => p.propertyName >= propertyValue
                case LinqExpressionType.ThanOrEqual:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(member, constant), parameter);
                    break;
                // p => p.propertyName <= propertyValue
                case LinqExpressionType.LessThanOrEqual:
                    expression = Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(member, constant), parameter);
                    break;
                //   p => p.propertyName.Contains(propertyValue)
                // p => !p.propertyName.Contains(propertyValue)
                case LinqExpressionType.Contains:
                case LinqExpressionType.NotContains:
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    constant = Expression.Constant(propertyValue, typeof(string));
                    if (expressionType == LinqExpressionType.Contains)
                    {
                        expression = Expression.Lambda<Func<T, bool>>(Expression.Call(member, method, constant), parameter);
                    }
                    else
                    {
                        expression = Expression.Lambda<Func<T, bool>>(Expression.Not(Expression.Call(member, method, constant)), parameter);
                    }
                    break;
                default:
                    // p => p.false
                    expression = False<T>();
                    break;
            }
            return expression;
        }

        /// <summary>
        /// 创建lambda表达式：p=>true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return p => true;
        }

        /// <summary>
        /// 创建lambda表达式：p=>false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>()
        {

            return p => false;
        }


        /// <summary>
        /// 解析多字段排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="orderBySelector">string=排序的字段,bool=true降序/false升序</param>
        /// <returns></returns>
        public static IQueryable<TEntity> GetIQueryableOrderBy<TEntity>(this IQueryable<TEntity> queryable, Dictionary<string, QueryOrderBy> orderBySelector)
        {
            string[] orderByKeys = orderBySelector.Select(x => x.Key).ToArray();
            if (orderByKeys == null || orderByKeys.Length == 0) return queryable;

            IOrderedQueryable<TEntity> queryableOrderBy = null;
            //  string orderByKey = orderByKeys[^1];
            string orderByKey = orderByKeys[orderByKeys.Length - 1];
            queryableOrderBy = orderBySelector[orderByKey] == QueryOrderBy.Desc
                ? queryableOrderBy = queryable.OrderByDescending(orderByKey.GetExpression<TEntity>())
                : queryable.OrderBy(orderByKey.GetExpression<TEntity>());

            for (int i = orderByKeys.Length - 2; i >= 0; i--)
            {
                queryableOrderBy = orderBySelector[orderByKeys[i]] == QueryOrderBy.Desc
                    ? queryableOrderBy.ThenByDescending(orderByKeys[i].GetExpression<TEntity>())
                    : queryableOrderBy.ThenBy(orderByKeys[i].GetExpression<TEntity>());
            }
            return queryableOrderBy;
        }

        /// <summary>
        /// 创建lambda表达式：p=>false
        /// 在已知TKey字段类型时,如动态排序OrderBy(x=>x.ID)会用到此功能,返回的就是x=>x.ID
        /// Expression<Func<Out_Scheduling, DateTime>> expression = x => x.CreateDate;指定了类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, TKey>> GetExpression<T, TKey>(this string propertyName, ParameterExpression parameter)
        {
            if (typeof(TKey).Name == "Object")
                return Expression.Lambda<Func<T, TKey>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(object)), parameter);
            return Expression.Lambda<Func<T, TKey>>(Expression.Property(parameter, propertyName), parameter);
        }
        /// <summary>
        /// 创建lambda表达式：p=>false
        /// object不能确认字段类型(datetime,int,string),如动态排序OrderBy(x=>x.ID)会用到此功能,返回的就是x=>x.ID
        /// Expression<Func<Out_Scheduling, object>> expression = x => x.CreateDate;任意类型的字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetExpression<T>(this string propertyName)
        {
            return propertyName.GetExpression<T, object>(typeof(T).GetExpressionParameter());
        }

        public static Expression<Func<T, object>> GetExpression<T>(this string propertyName, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(parameter, propertyName), typeof(object)), parameter);
        }

        public static ParameterExpression GetExpressionParameter(this Type type)
        {

            return Expression.Parameter(type, "p");
        }

    }

}
