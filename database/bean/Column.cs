using System;

namespace database.bean {
    public sealed class ColumnAttribute : Attribute {

        public ColumnAttribute() {
            this.Name = "";
            this.ColumnDefinition = "";
        }

        public ColumnAttribute(String name, String columnDefinition) {
            this.Name = name;
            this.ColumnDefinition = columnDefinition;
            this.IsPrimaryKey = false;
        }

        public ColumnAttribute(String name, String columnDefinition, bool primaryKey) {
            this.Name = name;
            this.ColumnDefinition = columnDefinition;
            this.IsPrimaryKey = primaryKey;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 列定义描述
        /// </summary>
        public String ColumnDefinition { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }
}
