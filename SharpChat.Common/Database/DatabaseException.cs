using System;

namespace SharpChat.Database {
    public class DatabaseException : Exception {}

    public class InvalidParameterClassTypeException : DatabaseException { }
}
