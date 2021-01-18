using System;

namespace database {
    public class User {
        public String UserName { get; set; }

        public String PassWord { get; set; }

        public override String ToString() {
            return String.Format("{{{0},{1}}}", UserName, PassWord);
        }
    }
}
