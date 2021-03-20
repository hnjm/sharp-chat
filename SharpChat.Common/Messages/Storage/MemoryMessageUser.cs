using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Messages.Storage {
    public class MemoryMessageUser : IUser {
        public long UserId => throw new NotImplementedException();
        public string UserName => throw new NotImplementedException();
        public Colour Colour => throw new NotImplementedException();
        public int Rank => throw new NotImplementedException();
        public string NickName => throw new NotImplementedException();
        public UserPermissions Permissions => throw new NotImplementedException();
        public UserStatus Status => throw new NotImplementedException();
        public string StatusMessage => throw new NotImplementedException();

        public bool Can(UserPermissions perm) {
            throw new NotImplementedException();
        }

        public bool Equals(IUser other) {
            throw new NotImplementedException();
        }
    }
}
