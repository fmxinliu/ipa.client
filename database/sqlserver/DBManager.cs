using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using database.bean;

namespace database.sqlserver {
    public class DBManager {
        protected String connectionString1 = "Data Source={0};Initial Catalog={1};Integrated Security=True;";
        protected String connectionString2 = "Data Source={0};Initial Catalog={1};User ID={2};Password={3};";
        protected String connectionString3 = "Data Source={0};Initial Catalog=master;Integrated Security=True;";
        protected String connectionString4 = "Data Source={0};Initial Catalog=master;User ID={2};Password={3};";
        private static readonly String defaultDbDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly String defaultDbName = "SqlServerHelper.MDF";
        private static readonly String dataSource = @".\SQLEXPRESS";
        private String dbName = String.Empty;

        protected DBManager() : this(defaultDbName) { }

        protected DBManager(String dbName) {
            if (!String.IsNullOrWhiteSpace(dbName)) {
                if (!dbName.Contains("\\")) {
                    dbName = defaultDbDir + dbName;
                }
            }
            else {
                dbName = defaultDbDir + defaultDbName;
            }
            this.dbName = dbName;

            // 数据库连接字符串
            this.connectionString1 = String.Format(
                this.connectionString1, dataSource, Path.GetFileNameWithoutExtension(dbName));

            // master连接字符串
            this.connectionString3 = String.Format(
                this.connectionString3, dataSource, "master");

            // 连接master
            this.createDB(getDBName());
        }

        protected String getDBName() {
            return this.dbName;
        }

        /// <summary>
        /// 数据库是否存在
        /// </summary>
        public Boolean dbExists(String dbName) {
            bool exist = false;
            using (var dr = SQLServerHelper.ExecuteReader(
                String.Format(connectionString3, dataSource),
                String.Format(Sql.DB_EXISTS, Path.GetFileNameWithoutExtension(dbName), Path.GetDirectoryName(dbName)),
                CommandType.Text)) {
                    exist = dr != null && dr.HasRows;
            }

            // 数据库文件被手动删除，drop一次
            if (exist && !File.Exists(dbName)) {
                try {
                    SQLServerHelper.ExecuteNonQuery(
                        String.Format(connectionString3, dataSource),
                        String.Format(Sql.DELETE_DB, Path.GetFileNameWithoutExtension(dbName)),
                        CommandType.Text);
                }
                catch (SqlException) { }
                finally {
                    exist = false;
                }
            }

            return exist;
        }

        /// <summary>
        /// 当前连接，是否存在表
        /// </summary>
        public Boolean tableExists(String tableName) {
            return this.tableExists(getDBName(), tableName);
        }

        /// <summary>
        /// 当前连接，是否存在表
        /// </summary>
        public Boolean tableExists(String dbName, String tableName) {
            using (var dr = SQLServerHelper.ExecuteReader(
                String.Format(connectionString1, dataSource),
                String.Format(Sql.TABLE_EXISTS, tableName),
                CommandType.Text)) {
                return dr != null && dr.HasRows;
            }
        }

        /// <summary>
        /// 获取数据库中所有表名
        /// </summary>
        public List<String> getTableNames(String dbName) {
            List<String> tables = new List<String>();
            var dr = SQLServerHelper.ExecuteReader(
                String.Format(connectionString1, dataSource),
                Sql.GET_ALL_TABLE_NAME,
                CommandType.Text);
            while (dr != null && dr.Read()) {
                tables.Add(dr[0].ToString());
            }
            return tables;
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public void createDB(String dbName) {
            // 查询数据库是否存在
            bool exist = dbExists(dbName);
            if (!exist) {
                // 创建数据库
                SQLServerHelper.ExecuteNonQuery(
                    String.Format(connectionString3, dataSource),
                    String.Format(Sql.CREATE_DB, Path.GetFileNameWithoutExtension(dbName), dbName.Replace(".MDF", "")),
                    CommandType.Text);
                // 数据库是否创建成功
                exist = File.Exists(dbName);
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        public Boolean createTable(String dbName, String tableName, Type type) {
            Int32 row = SQLServerHelper.ExecuteNonQuery(
                String.Format(connectionString1, dataSource),
                String.Format(Sql.CREATE_TABLE, tableName, getTableColumnDefinition(type)),
                CommandType.Text); 
            //using (var dr = SQLServerHelper.ExecuteReader(
            //    String.Format(connectionString1, dataSource),
            //    String.Format(Sql.CREATE_TABLE, getFullTableName(dbName, tableName), getTableColumnDefinition(type)),
            //    CommandType.Text)) {}
            return tableExists(dbName, tableName);
        }

        /// <summary>
        /// 通过反射，获取表定义
        /// </summary>
        public String getTableColumnDefinition(Type type) {
            String columnString = null;
            List<String> keys = new List<String>();
            StringBuilder sb = new StringBuilder();

            // 遍历属性特性，生成列
            foreach (PropertyInfo pi in type.GetProperties()) {
                foreach (Attribute attr in pi.GetCustomAttributes(true)) {
                    ColumnAttribute column = attr as ColumnAttribute;
                    if (column != null) {
                        System.Diagnostics.Debug.Assert(column.Name != null);
                        System.Diagnostics.Debug.Assert(column.ColumnDefinition != null);
                        sb.AppendFormat("[{0}] {1},", column.Name, column.ColumnDefinition);
                    }
                }
            }

            columnString = sb.ToString(0, sb.Length - 1);
            return columnString;
        }

        public String getFullTableName(String dbName, String tableName) {
            return Path.GetFileNameWithoutExtension(dbName) + ".dbo." + tableName;
        }
    }
}
