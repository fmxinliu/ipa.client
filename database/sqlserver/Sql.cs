using System;

namespace database.sqlserver {
    public static class Sql {
        public readonly static String TABLE_EXISTS = "select * from dbo.SysObjects where ID=object_id(N'{0}') and objectproperty(ID, N'IsTable') = 1";
        public readonly static String GET_ALL_TABLE_NAME = "select name from dbo.SysObjects where objectproperty(ID, N'IsUserTable') = 1";
        public readonly static String CREATE_TABLE = "create table {0} ({1})";
        public readonly static String DELETE_TABLE = "drop table {0}";

        public readonly static String DB_EXISTS = "select * from sys.databases where name='{0}'";
        public readonly static String CREATE_DB = "create database {0} on (name='{0}', filename='{1}.MDF')";
        public readonly static String ATTACH_DB = "exec sp_attach_dbc @dbname='{0}' @filename1='{1}.MDF', @filename2='{1}.LDF'";
        public readonly static String DELETE_DB = "drop database {0}";
    }
}
