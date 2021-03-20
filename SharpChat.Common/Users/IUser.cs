using System;

namespace SharpChat.Users {
    public interface IUser : IEquatable<IUser> {
        /// <summary>
        /// Unique ID of the user
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// Default unique name of the user
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Display colour of the user
        /// </summary>
        Colour Colour { get; }

        /// <summary>
        /// Hierarchical rank of the user
        /// </summary>
        int Rank { get; }

        /// <summary>
        /// Temporary alternate display name for the user
        /// </summary>
        string NickName { get; }

        /// <summary>
        /// Permissions the user has
        /// </summary>
        UserPermissions Permissions { get; }

        /// <summary>
        /// Current presence status of the user
        /// </summary>
        UserStatus Status { get; }

        /// <summary>
        /// Current presence message of the user
        /// </summary>
        string StatusMessage { get; }

        /// <summary>
        /// Permission check shorthand
        /// </summary>
        bool Can(UserPermissions perm);
    }
}
