using BugsMVC.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Dynamic;

namespace BugsMVC.Extensions
{
    public static class QueriableJQGridExtensions
    {
        public static IQueryable<T> GetFiltersFromJQGrid<T>(this IQueryable<T> query, string filters) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(filters))
                {
                    return query;
                }

                var itemFilters = JQGridFilterRule.Parse(filters);

                foreach (var item in itemFilters)
                {
                    if (item.op == "cn")
                    {
                        var property = typeof(T).GetProperty(item.field);

                        query = query.Where(string.Format("{0}.ToUpper().Contains(@0)", item.field), item.data.ToUpper());
                    }

                    if (item.op == "eq")
                    {
                        var property = typeof(T).GetProperty(item.field);

                        if (property == null)
                        {
                            return query;
                        }

                        if (property.PropertyType == typeof(DateTime))
                        {
                            DateTime value;
                            if (!DateTime.TryParse(item.data, out value))
                            {
                                return query;
                            }

                            query = query.Where(string.Format("{0} = @0", item.field), value);
                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            int value;
                            if (!int.TryParse(item.data, out value))
                            {
                                return query;
                            }

                            query = query.Where(string.Format("{0}=@0", item.field), value);
                        }
                        else if (property.PropertyType == typeof(decimal))
                        {
                            decimal value;
                            if (!decimal.TryParse(item.data, out value))
                            {
                                return query;
                            }

                            query = query.Where(string.Format("{0}=@0", item.field), value);
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            bool value;
                            if (!bool.TryParse(item.data, out value))
                            {
                                return query;
                            }
                            query = query.Where(string.Format("{0}=@0", item.field), value);
                        }
                        else
                        {
                            query = query.Where(string.Format("{0}.ToUpper() = @0", item.field), item.data.ToUpper());
                        }
                    }
                }

                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static IQueryable<T> GetSortFromJQGrid<T>(this IQueryable<T> query, string sidx, string sord)
        {
            try
            {
                if (sidx != "" && sidx != null)
                {
                    var property = typeof(T).GetProperty(sidx);
                    if (property != null)
                    {
                        return query.OrderBy(string.Format("{0} {1}", property.Name, sord));
                    }
                }

                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static IQueryable<T> PageJQGrid<T>(this IQueryable<T> query, int page, int rows, out int totalPages, out int totalRecords)
        {
            try
            {
                totalRecords = query.Count();

                totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

                int pageIndex = Convert.ToInt32(page) - 1;

                int pageSize = rows;

                return query.Skip(pageIndex * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}