using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BugsMVC.Helpers
{
    public class JQGridPostData
    {
        public bool _search { get; set; }
        public string nd { get; set; }
        public int rows { get; set; }
        public int page { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public JQGridFilters filters { get; set; }
    }

    public class JQGridFilters
    {
        public string groupOp { get; set; }
        public IList<JQGridFilterRule> rules { get; set; }
    }

    public class JQGridFilterRule
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }

        public static class Keys
        {
            public static class Usuario
            {
                public static readonly string FILTERS = "usuario_filters";
                public static readonly string SIDX = "usuario_sidx";
                public static readonly string SORD = "usuario_sord";
                public static readonly string PAGE = "usuario_page";
            }

            public static class Transaccion
            {
                public static readonly string FILTERS = "transaccion_filters";
                public static readonly string SIDX = "transaccion_sidx";
                public static readonly string SORD = "transaccion_sord";
                public static readonly string PAGE = "transaccion_page";
            }

            public static class MercadoPago
            {
                public static readonly string FILTERS = "mercadopago_filters";
                public static readonly string SIDX = "mercadopago_sidx";
                public static readonly string SORD = "mercadopago_sord";
                public static readonly string PAGE = "mercadopago_page";
            }
        }

        public static List<JQGridFilterRule> Parse(string filters)
        {
            if (string.IsNullOrEmpty(filters))
            {
                return new List<JQGridFilterRule>();
            }
            JObject data = JsonConvert.DeserializeObject<JObject>(filters);
            //JToken jQFilters = data.GetValue("filters");
            //if (jQFilters == null)
            //{
            //    return new List<JQFilterRule>();
            //}

            var rules = (data).GetValue("rules");

            return rules.Select(x => x.ToObject<JQGridFilterRule>()).ToList();
        }
    }

    public class JQGridQueryBuilder<T>
    {
        public JQGridQueryBuilder()
        {
        }

        Dictionary<string, string> _additinalMaps = new Dictionary<string, string>();
        string _query = string.Empty;
        string _defaultSortField = "ID";
        private string _where;
        private string _sort;
        private string _pagination;

        public string Query()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(this._query))
            {
                return string.Empty;
            }

            sb.AppendLine(this._query);

            if (!string.IsNullOrEmpty(this._where))
            {
                sb.AppendLine(string.Format("WHERE {0}", this._where));
            }

            if (!string.IsNullOrEmpty(this._sort))
            {
                sb.AppendLine(this._sort);
            }

            if (!string.IsNullOrEmpty(this._pagination))
            {
                sb.AppendLine(this._pagination);
            }

            return sb.ToString();
        }

        public JQGridQueryBuilder<T> WithBaseQuery(string baseQuery)
        {
            this._query = baseQuery;
            return this;
        }

        public JQGridQueryBuilder<T> AddColumnMap(string columnName, string sqlMap)
        {
            _additinalMaps.Add(columnName, sqlMap);
            return this;
        }

        public JQGridQueryBuilder<T> WithDefaultSortField(string value)
        {
            this._defaultSortField = value;
            return this;
        }

        public JQGridQueryBuilder<T> WithFilters(string filters)
        {
            try
            {
                if (string.IsNullOrEmpty(filters) || string.IsNullOrEmpty(this._query))
                {
                    return this;
                }

                string whereQuery = string.Empty;

                var itemFilters = JQGridFilterRule.Parse(filters);

                foreach (var item in itemFilters)
                {
                    var property = typeof(T).GetProperty(item.field);

                    if (property == null)
                    {
                        return this;
                    }
                    string columnName = string.Empty;
                    string mappedColumnValue = string.Empty;
                    if (this._additinalMaps.TryGetValue(item.field, out mappedColumnValue))
                    {
                        columnName = mappedColumnValue;
                    }
                    else
                    {
                        columnName = item.field;
                    }

                    if (item.op == "cn")
                    {

                        if (string.IsNullOrEmpty(whereQuery))
                        {
                            whereQuery = string.Format("{0} like'%{1}%'", columnName, item.data.ToUpper());
                        }
                        else
                        {
                            whereQuery = whereQuery + " and " + string.Format("{0} like'%{1}%'", columnName, item.data.ToUpper());
                        }
                    }

                    if (item.op == "eq")
                    {

                        if (property.PropertyType == typeof(DateTime))
                        {
                            DateTime value;
                            if (!DateTime.TryParse(item.data, out value))
                            {
                                return this;
                            }

                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0}  = '{1}'", columnName, value.ToString("yyyyMMdd"));
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0}  = '{1}'", columnName, value.ToString("yyyyMMdd"));
                            }

                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            int value;
                            if (!int.TryParse(item.data, out value))
                            {
                                return this;
                            }

                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0} = {1}", columnName, value);
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0} = {1}", columnName, value);
                            }
                        }
                        else if (property.PropertyType == typeof(int?))
                        {
                            int value;
                            if (!int.TryParse(item.data, out value))
                            {
                                return this;
                            }

                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0} = {1}", columnName, value);
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0} = {1}", columnName, value);
                            }
                        }
                        else if (property.PropertyType == typeof(decimal))
                        {
                            decimal value;
                            if (!decimal.TryParse(item.data, out value))
                            {
                                return this;
                            }

                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0}  = {1}", columnName, value);
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0} = {1}", columnName, value);
                            }
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            bool value;
                            if (!bool.TryParse(item.data, out value))
                            {
                                return this;
                            }

                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0}  = {1}", columnName, value ? 1 : 0);
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0} = {1}", columnName, value ? 1 : 0);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(whereQuery))
                            {
                                whereQuery = string.Format("{0}  = '{1}'", columnName, item.data.ToUpper());
                            }
                            else
                            {
                                whereQuery = whereQuery + " and " + string.Format("{0} = '{1}'", columnName, item.data.ToUpper());
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(this._where))
                {
                    this._where = whereQuery;
                }
                else
                {
                    this._where = this._where + " and " + whereQuery;
                }

                return this;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JQGridQueryBuilder<T> WithCustomFilters(string condition)
        {
            try
            {
                if (string.IsNullOrEmpty(this._where))
                {
                    this._where = condition;
                }
                else
                {
                    this._where = this._where + " and " + condition;
                }

                return this;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JQGridQueryBuilder<T> WithSort(string sidx, string sord)
        {
            if (string.IsNullOrEmpty(sidx))
            {
                sidx = this._defaultSortField;
            }

            if (string.IsNullOrEmpty(sord))
            {
                sord = "asc";
            }

            this._sort = string.Format("ORDER BY {0} {1}", sidx, sord);
            return this;
        }

        public JQGridQueryBuilder<T> WithPagination(int page, int rows)
        {
            int pageIndex = Convert.ToInt32(page) - 1;

            int pageSize = rows;

            this._pagination = string.Format("OFFSET {0} * ({1}) ROWS FETCH NEXT {0} ROWS ONLY OPTION(RECOMPILE)", pageSize, pageIndex);

            return this;
        }
    }

}