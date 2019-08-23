# Sock Chat Protocol Information

The protocol operates on a websocket in text mode. Messages sent between the client and server are a series of concatenated strings delimited by the vertical tab character, represented in most languages by the escape sequence `\t` and defined in ASCII as `0x09`.
The first string in this concatenation must be the packet identifier, sent as an integer. The packet identifiers are as follows.

## Client

- `0`: User keepalive ping. Done to prevent the client from closing the session due to socket inactivity. The only parameter of this is the user id.
- `1`: User join request. Takes a variable number of parameters that are fed into the authentication script associated with the chat.
- `2`: User message. The first parameter is the user id, the second is the message.

## Server

- `0`: Keepalive ping response. Not actually handled, but the client does receive it. The first and only parameter is always the string `pong`.
- `1`: User joining message. Takes two different forms depending on the recipient.
    - If the recipient is not the joining user, the arguments are as follows.
        - Unix Timestamp
        - User Id
        - Username
        - User Colour
        - Permission String
        - Message Id
    - If the recipient is the joining user, the arguments depend on the first parameter of the message.
        - If `y` (connection accepted), the parameters are as follows:
            - User Id
            - Username
            - User Colour
            - Permission String
            - Default Channel
        - If `n` (connection refused), the parameters are as follows:
            - REASON ENUM
            - `authfail`: Auth data is wrong.
            - `userfail`: Username is in use.
            - `sockfail`: Socket session has already started.
            - `joinfail`: Banned.
                - Followed by unix timestamp.
- `2`: Chat message to be parsed. Takes a general form but has a special case for bot messages.
    - Formatted as follows:
        - Unix Timestamp
        - User Id
        - Message (sanitised on the server side)
        - Message Id
        - Message Flags
    - If the message is sent by the chat bot, the User Id will be set to -1, and the MESSAGE field will be a series of string concatenated by the form feed operator, ASCII character `0x0C` and represented in most languages by the escape sequence `\f`. The string will take the following form:
        - MESSAGE TYPE ENUM
            - `0`: Normal message.
            - `1`: Error message.
        - Id of string in legacy language files.
        - Any number of parameters, fed as arguments into the language string.
- `3`: User disconnect notification. The parameters are as follows:
    - User Id
    - Username
    - Departure reason, can technically be anything but Railgun sticks to the following ones as they were implemented by the legacy client:
        - `kick`, for both bans and kicks.
        - `flood`, for flood protection kicks.
        - `leave`, for regular departures.
    - Unix timestamp
    - Message Id
- `4`: Channel event notification to client. First parameter is an integer representing the action taken, and are described below.
    - `0`: Creation, parameters are as follows:
        - Channel name
        - Password protected, (either `0` or `1`)
        - Temporary, (same as above)
    - `1`: Update,
        - Old channel name
        - New channel name
        - Password protected
        - Temporary
    - `2`: Delete,
        - Channel name
- `5`: User changing channel information for clients. First parameter is an integer representing the action taken, and are described below.
    - `0`: User joining, parameters are as follows.
        - User Id
        - Username
        - User Colour
        - Message id
    - `1`: User leaving.
        - User Id
        - Message Id
    - `2`: Telling the client that it has been moved to a different channel.
        - Channel Name
- `6`: Indicates that a message has been removed, it only has a single argument which is the id of the message that has been removed.
- `7`: Indicates the sending of data about the context of the server to the client. It has a number of actions described below.
    - `0`: The users in the current channel, formatted as follows:
        - Number of users, represented as `n`.
        - `n` repetitions of the following parameters, each indicating a user:
            - User Id
            - Username
            - User Colour
            - Permission String
            - Visibility (`0` or `1`)
    - `1`: A message object, the parameters are as follows:
        - Unix timestamp
        - User Id
        - Username
        - User Colour
        - Permission string
        - Message
        - Message Id
        - Should play a sound on receive (`0` or `1`)
        - Message flags
    - `2`: The server's channels, formatted as follows:
        - The number of channels, represented as `n`.
        - `n` repetitions of the following parameters, each indicating a channel:
            - Channel name
            - Password protected, `0` or `1`.
            - Temporary, same as above.
- `8`: Forces a context clear on the client. The first parameters is an integer ranging from `0` to `4`.
    - `0`: Clears only the message listing.
    - `1`: Clears only the user listing.
    - `2`: Clears only the channel listing.
    - `3`: Clears both the message and user listings.
    - `4`: Clear all three listings.
- `9`: Tells the client that their connection is about to forcefully terminated, used to indicate kick and bans.
    - `0`: The user has been kicked.
    - `1`: The user has been banned, this comes with another argument:
        - Unix timestamp indicating the length of the ban.
- `10`: Informs the client that a user (usually in the same channel) has been updated. Used for things like nicknames and temporary privilege elevations. The arguments are as follows:
    - User Id, this never changes and can thus be used to indentify the user.
    - Username
    - User Colour
    - Permission string.

## User Permission String
The User Permission String consists out of five (5) parts concatenated by the form feed operator, indentified in most languages as the escape sequence `\f` and defined as the ASCII character `0x0C`.
In the original specification it appeared as if custom permission flags were possible, these have always gone completely unused and should thus be avoided.
The parts are as follows:

- An integer indicating the hierarchy of the user, this is used to determine whether a user has access to certain channels or is able to act out moderator actions upon certain users (lower can't affect higher).
- A boolean indicating whether the user has the ability to kick people.
- A boolean indicating whether the user has access to the logs, this should be zero unless the client has direct access to the message history without a connection the actual chat.
- A boolean indicating whether the user is able to change their nick/display name.
- An integer ranging from 0 to 2 indicating whether the user can create channels
    - `0`: User cannot create channels.
    - `1`: User can create channels, but only temporary ones. These _usually_ disappear after the last user left.
    - `2`: User can create permanent channels.

## Message Flags
The Message Flags alter how a message should be displayed to the client, these are all boolean values.
I'm not entirely sure if these allowed for custom flags, but much like the custom flags in the User Permission String, these have gone unused and should thus, also, be avoided.
The parts are as follows:

- Username should appear using a **bold** font.
- Username should appear using a *cursive* font.
- Username should appear __underlined__.
- A colon `:` should be displayed between the username and the message.
- The message was sent privately, directly to the current user.

As an example, the most common message flagset is `10010`.
