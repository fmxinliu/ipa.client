using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace database {
    public static class DapperHelper {
        private static String connectionString;
        public static String ConnectionString {
            get {
                return connectionString;
            }
            set {
                connectionString = value;
            }
        }

        /// <summary>
        /// 插入单条记录
        /// </summary>
        public static Boolean Insert(User user) {
            using (IDbConnection conn = new SqlConnection(connectionString)) {
                var affectedRows = conn.Execute("insert into [User] values (@UserName, @PassWord)", user);
                return affectedRows > 0;
            }
        }

        /// <summary>
        /// 批量插入(Bulk)
        /// </summary>
        public static Boolean Insert(IEnumerable<User> userList) {
            using (IDbConnection conn = new SqlConnection(connectionString)) {
                var affectedRows = conn.Execute("insert into [User] values (@UserName, @PassWord)", userList);
                return affectedRows == userList.Count();
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public static Boolean Update(User user) {
            using (IDbConnection conn = new SqlConnection(connectionString)) {
                var affectedRows = conn.Execute("update [User] set PassWord=@PassWord where UserName=@UserName", user);
                return affectedRows > 0;
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        public static IEnumerable<User> Query(User user) {
            using (IDbConnection conn = new SqlConnection(connectionString)) {
                return conn.Query<User>("select * from [User] where UserName=@UserName", user);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        public static Boolean Delete(User user) {
            using (IDbConnection conn = new SqlConnection(connectionString)) {
                var affectedRows = conn.Execute("delete from [User] where UserName=@UserName", user);
                return affectedRows > 0;
            }
        }
    }
}
