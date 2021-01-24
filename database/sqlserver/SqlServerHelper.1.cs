using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace database.sqlserver {
    public static partial class SQLServerHelper {
        public static String ConnectionString { get; set; }
        public static String GetConnectionStringByConfig() {
            return ConfigurationManager.ConnectionStrings["SqlServerHelper"].ConnectionString;
        }

        private static readonly String error = "类'{0}'未定义'{1}'属性，请调用：{2}，指定主键'{1}'。";

        /// <summary>
        /// 根据id查询记录
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <typeparam name="I">唯一主键类型(Int32或String)</typeparam>
        /// <param name="keyValue">主键值</param>
        /// <param name="keyName">主键名</param>
        /// <returns>结果</returns>
        public static T QueryById<T, I>(I keyValue, String keyName = "Id") {
            Type type = typeof(T);
            String columnString = String.Join(",", type.GetProperties().Select(p => String.Format("[{0}]", p.Name)));
            String sqlString = String.Format("select {0} from [{1}] where {2}={3}", columnString, type.Name, keyName,
                keyValue.GetType() == typeof(String) ? "'" + keyValue.ToString() + "'" : keyValue.ToString());
            T obj = default(T);
            using (SqlDataReader sdr = ExecuteReader(ConnectionString, sqlString, CommandType.Text)) {
                if (sdr != null && sdr.Read()) {
                    obj = (T)CreateInstance(type, sdr);
                }
            }
            return obj;
        }

        /// <summary>
        /// 查询所有记录
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>结果集</returns>
        public static List<T> QueryAll<T>() {
            Type type = typeof(T);
            String columnString = String.Join(",", type.GetProperties().Select(p => String.Format("[{0}]", p.Name)));
            String sqlString = String.Format("select {0} from [{1}]", columnString, type.Name);
            List<T> dataList = new List<T>();
            using (SqlDataReader sdr = ExecuteReader(ConnectionString, sqlString, CommandType.Text)) {
                while (sdr != null && sdr.Read()) {
                    dataList.Add((T)CreateInstance(type, sdr));
                }
            }
            return dataList;
        }

        /// <summary>
        /// 插入记录
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象实例</param>
        /// <param name="keyName">自增主键(唯一)</param>
        /// <returns>是否插入成功</returns>
        public static Boolean Insert<T>(T t, String keyName = "Id") {
            Type type = typeof(T);
            String columnString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("[{0}]", p.Name)));
            String valueString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("@{0}", p.Name)));
            String sqlString = String.Format("insert [{0}] ({1}) values ({2})", type.Name, columnString, valueString);
            SqlParameter[] sqlParameter = type.GetProperties()
                .Where(p => p.Name != keyName)
                .Select(p => new SqlParameter(String.Format("@{0}", p.Name), p.GetValue(t, null) ?? DBNull.Value))
                .ToArray();
            return ExecuteNonQuery(ConnectionString, sqlString, CommandType.Text, sqlParameter) > 0;
        }

        /// <summary>
        /// 更新记录(对象需定义主键属性)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <param name="keyName">主键名</param>
        /// <returns>是否更新成功</returns>
        public static Boolean Update<T>(T t, String keyName = "Id") {
            Type type = typeof(T);
            var exist = type.GetProperties().FirstOrDefault(p => p.Name == keyName);
            if (exist == null) {
                Debug.Assert(false, String.Format(error, type.Name, keyName, "public static Boolean Update<T, I>(T t, I keyValue, String keyName = \"Id\")"));
                return false;
            }
            String setString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("[{0}]=@{0}", p.Name)));
            String valueString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("@{0}", p.Name)));
            String sqlString = String.Format("update [{0}] set {1} where {2}={3}", type.Name, setString, keyName, "@" + keyName);
            SqlParameter[] sqlParameter = type.GetProperties()
                .Select(p => new SqlParameter(String.Format("@{0}", p.Name), p.GetValue(t, null) ?? DBNull.Value))
                .ToArray();
            return ExecuteNonQuery(ConnectionString, sqlString, CommandType.Text, sqlParameter) > 0;
        }

        /// <summary>
        /// 更新指定记录
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <typeparam name="I">唯一主键类型(Int32或String)</typeparam>
        /// <param name="t">对象</param>
        /// <param name="keyValue">主键值</param>
        /// <param name="keyName">主键名</param>
        /// <returns>是否更新成功</returns>
        public static Boolean Update<T, I>(T t, I keyValue, String keyName = "Id") {
            Type type = typeof(T);
            String setString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("[{0}]=@{0}", p.Name)));
            String valueString = String.Join(",", type.GetProperties().Where(p => p.Name != keyName).Select(p => String.Format("@{0}", p.Name)));
            String sqlString = String.Format("update [{0}] set {1} where {2}={3}", type.Name, setString, keyName,
                keyValue.GetType() == typeof(String) ? "'" + keyValue.ToString() + "'" : keyValue.ToString());
            SqlParameter[] sqlParameter = type.GetProperties()
                .Where(p => p.Name != keyName)
                .Select(p => new SqlParameter(String.Format("@{0}", p.Name), p.GetValue(t, null) ?? DBNull.Value))
                .ToArray();
            return ExecuteNonQuery(ConnectionString, sqlString, CommandType.Text, sqlParameter) > 0;
        }

        /// <summary>
        /// 查询所有记录
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>结果集</returns>
        public static Boolean Delete<T, I>(I keyValue, String keyName = "Id") {
            Type type = typeof(T);
            String sqlString = String.Format("delete from [{0}] where {1}={2}", type.Name, keyName,
                keyValue.GetType() == typeof(String) ? "'" + keyValue.ToString() + "'" : keyValue.ToString());
            return ExecuteNonQuery(ConnectionString, sqlString, CommandType.Text) > 0;
        }

        public static Boolean Delete<T>(T t, String keyName = "Id") {
            Type type = typeof(T);
            String sqlString = String.Format("delete from [{0}] where {1}={2}", type.Name, keyName, "@" + keyName);
            SqlParameter[] sqlParameter = type.GetProperties()
                .Where(p => p.Name == keyName)
                .Select(p => new SqlParameter(String.Format("@{0}", p.Name), p.GetValue(t, null) ?? DBNull.Value))
                .ToArray();
            if (sqlParameter.Length == 0) {
                Debug.Assert(false, String.Format(error, type.Name, keyName, "public static Boolean Delete<T, I>(I keyValue, String keyName = \"Id\")"));
                return false;
            }
            return ExecuteNonQuery(ConnectionString, sqlString, CommandType.Text, sqlParameter) > 0;
        }

        private static Object CreateInstance(Type type, SqlDataReader sdr) {
            Object obj = Activator.CreateInstance(type);
            foreach (var item in type.GetProperties()) {
                if (sdr[item.Name] is DBNull) { // 判空
                    item.SetValue(obj, null, null);
                }
                else {
                    item.SetValue(obj, sdr[item.Name], null);
                }
            }
            return obj;
        }

        #region 获取变量、类型或成员的名称

        public static String nameof<T>(Expression<Func<T>> expr) {
            return nameof(null, expr);
        }

        public static String nameof<T>(this Object obj, Expression<Func<T>> expr) {
            var me = expr.Body as MemberExpression;
            if (me != null) {
                return me.Member.Name;
            }
            var t = typeof(T);
            return (t.IsClass && t.IsGenericType) ?
                t.Name.Substring(0, t.Name.IndexOf('`')) : // 泛型类，返回类名
                t.Name;
        }

        #endregion
    }
}
