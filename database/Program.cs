using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using database.sqlserver;

namespace database {
    class Program {
        static void Main(string[] args) {
            TestMSSql();
            Console.ReadKey();
        }

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
        }
    }
}
