using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using database.sqlserver;

namespace database {
    class Program {
        static void Main(String[] args) {
            TestNameof();
            TestMSSql();
            Console.ReadKey();
        }

        /// <summary>
        /// nameof获取变量、类型或成员的名称
        /// </summary>
        static void TestNameof() {
            String tableName = "table";
            String nameOfTestVariable;
            User user = new User { UserName = "test", PassWord = "test" };

            nameOfTestVariable = SQLServerHelper.nameof(() => tableName); // tableName
            nameOfTestVariable = SQLServerHelper.nameof(() => user); // user
            nameOfTestVariable = SQLServerHelper.nameof(() => user.PassWord); // PassWord
            nameOfTestVariable = SQLServerHelper.nameof(() => new User()); // User
            nameOfTestVariable = SQLServerHelper.nameof(() => new List<User>()); // List
        }

        /// <summary>
        /// SqlServer增删改查
        /// </summary>
        static void TestMSSql() {
            String username = "user";
            String password = "123";
            UserTable userTable = new UserTable();
            Int32 i = 0;
            for (i = 1; i < Int32.MaxValue; i++) {
                if (!userTable.IsExist(username + i.ToString())) {
                    break;
                }
            }

            User user1 = new User { UserName = username + i.ToString(), PassWord = password };
            Console.WriteLine("{0} insert {1}", user1, userTable.Insert(user1));

            User user2 = new User { UserName = username + (i + 1).ToString(), PassWord = password };
            Console.WriteLine("{0} insert {1}", user2, userTable.InsertSafe(user2));


            SQLServerHelper.ConnectionString = SQLServerHelper.GetConnectionStringByConfig();

            // 插入
            Boolean b = false;
            User user = new User { UserName = username + (i + 2).ToString(), PassWord = "test" };
            b = SQLServerHelper.Insert(user);

            // 查询所有
            List<User> list = SQLServerHelper.QueryAll<User>();

            // 获取刚插入的一条
            user = SQLServerHelper.QueryById<User, Int32>(list.Count);
            user.PassWord = "user";

            // 更新
            b = SQLServerHelper.Update(user); // !!!
            b = SQLServerHelper.Update<User, Int32>(user, list.Count);

            // 查询
            user = SQLServerHelper.QueryById<User, Int32>(list.Count);

            UserPo userPo = new UserPo(user);
            user = SQLServerHelper.QueryById<User, Int32>(list.Count, SQLServerHelper.nameof(() => userPo.Id));

            // 删除
            b = SQLServerHelper.Delete(user); // !!!
            b = SQLServerHelper.Delete<User, Int32>(list.Count);

            list = SQLServerHelper.QueryAll<User>();
        }
    }
}
