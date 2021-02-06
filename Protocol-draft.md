# Sock Chat Protocol Information
The protocol operates on a websocket in text mode. Messages sent between the client and server are a series of concatenated strings delimited by the vertical tab character, represented in most languages by the escape sequence `\t` and defined in ASCII as `0x09`.
The first string in this concatenation must be the packet identifier, sent as an `int`.

Newer versions of the protocol are implemented as extensions, a client for Version 1 should have no trouble using a server built for Version 2 as long as authentication is understood.

The current stable version of the protocol is **Version 1**.

## Types

### `bool`
A value that indicates a true or a false state. `0` represents false and anything non-`0` represents true, please stick to `1` for representing true though.

### `int`
Any number ranging from `-9007199254740991` to `9007199254740991`, `Number.MAX_SAFE_INTEGER` and `Number.MIN_SAFE_INTEGER` in JavaScript.

### `string`
Any printable unicode character, except `\t` which is used to separate packets.

### `timestamp`
Extends `int`, contains a second based UNIX timestamp.

### `channel name`
A `string` containing only alphanumeric characters (any case), `-` or `_`.

### `session id`
A `string` containing only alphanumeric characters (any case), `-` or `_`.

### `color`
Any valid value for the CSS `color` property.
Further documentation can be found [on MDN](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value).

### `message flags`
Message flags alter how a message should be displayed to the client, these are all `bool` values.
The parts are as follows:

 - Username should appear using a **bold** font.
 - Username should appear using a *cursive* font.
 - Username should appear __underlined__.
 - A colon `:` should be displayed between the username and the message.
 - The message was sent privately, directly to the current user.

As an example, the most common message flagset is `10010`. This indicates a bold username with a colon separator.

### `user permissions`
User permissions are a set of flags separated by either the form feed character (`\f` / `0x0C`) or a space (<code> </code> / `0x20`).
The reason there are two options is due to a past mixup that we now have to live with.
Which of the methods is used remains consistent per server however, so the result of a test can be cached.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Rank of the user. Used to determine what channels a user can access or what other users the user can moderate.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user the ability kick/ban/unban others.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user can access the logs. This should always be <code>0</code>, unless the client has a dedicated log view that can be accessed without connecting to the chat server.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the user can set an alternate display name.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Indicates whether the user can create channel. If <code>0</code> the user cannot create channels, if <code>1</code> the user can create channels but they are to disappear when all users have left it and if <code>2</code> the user can create channels that permanently stay in the channel assortment.</td>
        <td></td>
    </tr>
</table>

## Client Packets
These are the packets sent from the client to the server.

### Packet `0`: Ping
Used to prevent the client from closing the session due to inactivity.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the packet was sent to the server</td>
        <td>Added in version 2</td>
    </tr>
</table>

### Packet `1`: Authentication
Takes a variable number of parameters that are fed into the authentication script associated with the chat.

<table>
    <tr>
        <td><code>...string</code></td>
        <td>Any amount of data required to complete authentication</td>
        <td></td>
    </tr>
</table>

### Packet `2`: Message
Informs the server that the user has sent a message.

Commands are described lower in the document.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, may not contain <code>\t</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Name of the target channel</td>
        <td>Added in version 2 through the <code>MCHAN</code> capability. May be omitted to target the last addressed channel.</td>
    </tr>
</table>

### Packet `3`: Capabilities
Informs the server what capabilities this client supports. Capabilities can be adjusted at any time.

Added in Version 2.

<table>
    <tr>
        <td><code>string</code></td>
        <td>A space separated list of capability strings.</td>
        <td></td>
    </tr>
</table>

### Packet `4`: Typing
Informs the currently focussed channel that this client is writing a message.

Added in Version 2 through the <code>TYPING</code> capability.

<table>
    <tr>
        <td><code>string</code></td>
        <td>Name of the target channel. May be omitted to target the last addressed channel.</td>
        <td></td>
    </tr>
</table>

## Server Packets
These are the packets sent from the server to the client.

### Packet `0`: Pong
Response to client packet `0`: Ping.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the packet was sent to the client</td>
        <td>Originally this field contained the string <code>pong</code>, but this value was completely unused and can be safely replaced.</td>
    </tr>
</table>

### Packet `1`: Join/Auth
While authenticated this packet indicates that a new user has joined the server/channel. Before authentication this packet serves as a response to client packet `1`: Authentication.

#### Successful authentication response
Informs the client that authentication has succeeded.

<table>
    <tr>
        <td><code>string</code></td>
        <td><code>y</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Default channel the user will join following this packet</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Extensions version number. If this field is missing, version 1 is implied.</td>
        <td>Added in Version 2</td>
    </tr>
    <tr>
        <td><code>session id</code></td>
        <td>ID of the currently active session</td>
        <td>Added in Version 2</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Maximum permitted size of a chat message</td>
        <td>Added in Version 2</td>
    </tr>
</table>

#### Failed authentication response
Informs the client that authentication has failed.

<table>
    <tr>
        <td><code>string</code></td>
        <td><code>n</code></td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            Reason for failure.
            <ul>
                <li><code>authfail</code>: Authentication data is invalid.</li>
                <li><code>userfail</code>: Username in use.</li>
                <li><code>sockfail</code>: A connection has already been started (used to cap maximum connections to 5 in SharpChat).</li>
                <li><code>joinfail</code>: User attempting to authenticate is banned.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>If <code>joinfail</code> this contains expiration time of the ban, otherwise it is empty.</td>
        <td></td>
    </tr>
</table>

#### User joining
Informs the client that a user has joined.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time the user joined</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>colour</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>

### Packet `2`: Chat message
Informs the client that a chat message has been received.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the message was received by the server</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>
            User ID.
            If <code>-1</code> this message is an informational message from the server and the next field takes on a special form.
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            <p>Message, sanitised by the server</p>
            <p>
                If this is an informational message this field is formatted as follows and concatenated by the form feed character <code>\f</code>, respresented in ASCII by <code>0x0C</code>. Bot message formats are described lower in the document.
                <table>
                    <tr>
                        <td><code>int</code></td>
                        <td>
                            Message type.
                            <ul>
                                <li><code>0</code> for a normal informational message.</li>
                                <li><code>1</code> for an error.</li>
                            </ul>
                        </td>
                    </tr>
                    <tr>
                        <td><code>string</code></td>
                        <td>An id of a string in the legacy language files.</td>
                    </tr>
                    <tr>
                        <td><code>...string</code></td>
                        <td>Any number of parameters used to format the language string.</td>
                    </tr>
                </table>
            </p>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Channel this message was sent in</td>
        <td>Added in Version 2</td>
    </tr>
</table>

### Packet `3`: User disconnect
Informs the client that a user has disconnected.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            One of four disconnect reasons.
            <ul>
                <li><code>leave</code>: The user gracefully left, e.g. "x logged out".</li>
                <li><code>timeout</code>: The user lost connection unexpectedly, e.g. "x timed out".</li>
                <li><code>kick</code>: The user has been kicked, e.g. "x has been kicked".</li>
                <li><code>flood</code>: The user has been kicked for exceeding the flood protection limit, e.g. "x has been kicked for spam".</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the user disconnected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>

### Packet `4`: Channel event
This packet informs the user about channel related updates. The only consistent parameter across sub-packets is the first one described as follows.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Channel information update event ID.
            <ul>
                <li><code>0</code>: A channel has been created.</li>
                <li><code>1</code>: A channel has been updated.</li>
                <li><code>2</code>: A channel has been deleted.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>

#### Sub-packet `0`: Channel creation
Informs the client that a channel has been created.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>The name of the new channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `1`: Channel update
Informs the client that details of a channel has changed.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Old/current name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>New name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `2`: Channel deletion
Informs the client that a channel has been deleted

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel that has been deleted</td>
        <td></td>
    </tr>
</table>

### Packet `5`: Channel switching
This packet informs the client about channel switching.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Channel information update event ID.
            <ul>
                <li><code>0</code>: A user joined the channel. Sent to all users in said channel.</li>
                <li><code>1</code>: A user left the channel. Sent to all users in said channel.</li>
                <li><code>2</code>: The client is to forcibly join a channel.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>

#### Sub-packet `0`: Channel join
Informs the client that a user has joined the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `1`: Channel departure
Informs the client that a user has left the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `2`: Forced channel switch
Informs the client that it has been forcibly switched to a different channel.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>The name of the channel that the user has been switched to</td>
        <td></td>
    </tr>
</table>

### Packet `6`: Message deletion
Informs the client that a message has been deleted.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID of the deleted message</td>
        <td></td>
    </tr>
</table>

### Packet `7`: Context information
Informs the client about the context of a channel before the client was present.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Context event ID.
            <ul>
                <li><code>0</code>: Users present in the current channel.</li>
                <li><code>1</code>: A message already in the channel, occurs more than once per channel.</li>
                <li><code>2</code>: Channels on the server.</li>
            </ul>
        </td>
        <td></td>
    </tr>
</table>

#### Sub-packet `0`: Existing users
Informs the client about users already present in the channel.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Amount of users present in the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>Context User</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
        <td></td>
    </tr>
</table>

##### Context User object

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `1`: Existing message
Informs the client about an existing message in a channel.

<table>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the message was received by the server</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message</td>
        <td></td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags</td>
        <td></td>
    </tr>
</table>

#### Sub-packet `2`: Channels
Informs the client about the channels on the server.

<table>
    <tr>
        <td><code>int</code></td>
        <td>Amount of channels on the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>Context Channel</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
        <td></td>
    </tr>
</table>

##### Context Channel object

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
        <td></td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
        <td></td>
    </tr>
</table>

### Packet `8`: Context clearing
Informs the client that the context has been cleared.

<table>
    <tr>
        <td><code>int</code></td>
        <td>
            Number indicating what has been cleared.
            <ul>
                <li><code>0</code>: The message list has been cleared.</li>
                <li><code>1</code>: The user list has been cleared.</li>
                <li><code>2</code>: The channel list has been cleared.</li>
                <li><code>3</code>: Both the message and user listing have been cleared.</li>
                <li><code>4</code>: The message, user and channel listing have all been cleared.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>channel name</code></td>
        <td>Channel this clear is targeted towards. Ignore packet if this is set and channels are supposedly to be cleared. If this field is empty this packet is intended for the entire context.</td>
        <td>Added in Version 2</td>
    </tr>
</table>

### Packet `9`: Forced disconnect
Informs the client that they have either been banned or kicked from the server.

<table>
    <tr>
        <td><code>bool</code></td>
        <td>
            <ul>
                <li><code>0</code>: The client has been kicked, can reconnect immediately.</li>
                <li><code>1</code>: The client has been banned, can reconnect after the timestamp (documented below) in the next param has expired.</li>
            </ul>
        </td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Ban expiration time</td>
        <td></td>
    </tr>
</table>

### Packet `10`: User update
Informs that another user's details have been updated.

<table>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the affected user</td>
        <td></td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username</td>
        <td></td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>New username color</td>
        <td></td>
    </tr>
    <tr>
        <td><code>user permissions</code></td>
        <td>User permissions</td>
        <td></td>
    </tr>
</table>

### Packet `11`: Capability confirmation
Informs the client what capabilities the server understood from the Packet `3`.

Added in Version 2.

<table>
    <tr>
        <td><code>string</code></td>
        <td>A space separated string of accepted capability strings.</td>
        <td></td>
    </tr>
</table>

### Packet `12`: Typing Info
Informs the client that a user is typing.

Added in Version 2.

<table>
    <tr>
        <td><code>channel name</code></td>
        <td>Name of the channel in which the user is typing.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the typing user</td>
        <td></td>
    </tr>
    <tr>
        <td><code>timestamp</code></td>
        <td>Time when the user started typing.</td>
        <td></td>
    </tr>
</table>

## Capabilities
Capability strings sent in Client Packet `3` and Server Packet `11`.
If you wish to add your own unofficial capabilities please prefix them with `X_` to avoid conflicts.
Capability strings MUST be all uppercase, MUST only contain alphanumeric characters and underscores and MUST NOT start with a number. (Regular Expression: `[A-Z_][A-Z0-9_]+`)

### `TYPING`: Support for the Typing packets
While Client Packet `4` and Server Packet `12` are core features of Version 2, some people dislike typing indicators and this provides an easy way for servers to skip sending the packets to these users. The client may still inform the server the it is typing though ClientPacket `4`, but it won't receive Server Packet `12`.

### `MCHAN`: Multi-channel support
Allows the client to be present in multiple channels at once. Issuing `/join` will not cause the server to automatically make the user leave the channel.

## Bot Messages
Formatting IDs sent by user -1.

<table>
    <tr>
        <th colspan="3">Informational</th>
    </tr>
    <tr>
        <th>String</th>
        <th>Description</th>
        <th>Arguments</th>
    </tr>
    <tr>
        <td><code>say</code></td>
        <td>Just echoes the arguments in a message.</td>
        <td>The message.</td>
    </tr>
    <tr>
        <td><code>silence</code></td>
        <td>Informs the client that they have been silenced.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>unsil</code></td>
        <td>Informs the client that they are no longer silenced.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>silok</code></td>
        <td>Informs the client that they have successfully silenced another user.</td>
        <td>The username of the silenced user.</td>
    </tr>
    <tr>
        <td><code>usilok</code></td>
        <td>Informs the client that they have successfully removed the silencing from another user.</td>
        <td>The username of the unsilenced user.</td>
    </tr>
    <tr>
        <td><code>flwarn</code></td>
        <td>Informs the client that they are risking being kicking for flood protection (spam).</td>
        <td></td>
    </tr>
    <tr>
        <td><code>unban</code></td>
        <td>Informs the client that they have successfully removed the ban from a user or ip address.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>banlist</code></td>
        <td>Provides a list with banned users and IP addresses.</td>
        <td>
            Sets of "<code>&lt;a href="javascript:void(0);" onclick="Chat.SendMessageWrapper('/unban '+ this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of the banned user or the banned IP address. The set is separated by "<code>, </code>"
        </td>
    </tr>
    <tr>
        <td><code>who</code></td>
        <td>Provides a list of users currently online.</td>
        <td>
            Sets of "<code>&lt;a href="javascript:void(0);" onclick="UI.InsertChatText(this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of a user. The current online user is highlighted with "<code> style="font-weight: bold;"</code>" before the closing &gt; of the opening &lt;a&gt; tag. The set is separated by "<code>, </code>"
        </td>
    </tr>
    <tr>
        <td><code>whochan</code></td>
        <td>Provides a list of users currently online in a specific channel.</td>
        <td>
            Sets of "<code>&lt;a href="javascript:void(0);" onclick="UI.InsertChatText(this.innerHTML);"&gt;{0}&lt;/a&gt;</code>" where {0} is the username of a user. The current online user is highlighted with "<code> style="font-weight: bold;"</code>" before the closing &gt; of the opening &lt;a&gt; tag. The set is separated by "<code>, </code>"
        </td>
    </tr>
    <tr>
        <td><code>join</code></td>
        <td>Informs the client that a user connected with the server.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>jchan</code></td>
        <td>Informs the client that a user moved into the channel.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>leave</code></td>
        <td>Informs the client that a user disconnected from the server.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>lchan</code></td>
        <td>Informs the client that a user moved out of the channel.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>kick</code></td>
        <td>Informs the client that a user has disconnect because they got kicked.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>flood</code></td>
        <td>Informs the client that a user has disconnect because they got kicked for flood protection.</td>
        <td>The username of said user.</td>
    </tr>
    <tr>
        <td><code>nick</code></td>
        <td>Informs the client that a user has changed their nickname.</td>
        <td>The first argument is the previous username of said user, the second argument is their new username.</td>
    </tr>
    <tr>
        <td><code>crchan</code></td>
        <td>Informs the client that they have successfully created a channel.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>delchan</code></td>
        <td>Informs the client that they have successfully deleted a channel.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>cpwdchan</code></td>
        <td>Informs the client that they have successfully changed the password of the channel.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>cprivchan</code></td>
        <td>Informs the client that they have successfully changed the hierarchy level required for the channel.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>ipaddr</code></td>
        <td>Shows the IP address of another user.</td>
        <td>First argument is the username, second argument is the IP address.</td>
    </tr>
    <tr>
        <th colspan="3">Error</th>
    </tr>
    <tr>
        <th>String</th>
        <th>Description</th>
        <th>Arguments</th>
    </tr>
    <tr>
        <td><code>generr</code></td>
        <td>Generic fallback error.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>silerr</code></td>
        <td>Informs the client that the user they tried to silence had already been silenced.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>usilerr</code></td>
        <td>Informs the client that the user whose silence they tried to revoke hasn't been silenced.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>silperr</code></td>
        <td>Informs the client that they are not allowed to silence that user.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>usilperr</code></td>
        <td>Informs the client that they are not allowed to remove the silence from that user.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>silself</code></td>
        <td>Informs the client that they cannot silence themselves.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>delerr</code></td>
        <td>Informs the client that they are not allowed to delete a message.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>notban</code></td>
        <td>Informs the client that a username or IP address is not banned.</td>
        <td>The provided username or IP address.</td>
    </tr>
    <tr>
        <td><code>whoerr</code></td>
        <td>Informs the client that they do not have access to the channel they specified for the /who command.</td>
        <td>The provided channel name.</td>
    </tr>
    <tr>
        <td><code>cmdna</code></td>
        <td>Tells the client they're not allowed to use a command.</td>
        <td>First argument is the name of the command.</td>
    </tr>
    <tr>
        <td><code>nocmd</code></td>
        <td>Tells the client the command they tried to run does not exist.</td>
        <td>First argument is the name of the command.</td>
    </tr>
    <tr>
        <td><code>cmderr</code></td>
        <td>Tells the client that they formatted the last command incorrectly.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>usernf</code></td>
        <td>Tells the client that the user they requested was not found on the server.</td>
        <td>The requested username.</td>
    </tr>
    <tr>
        <td><code>kickna</code></td>
        <td>Tells the client that they are not allowed to kick a given user.</td>
        <td>Username of the user they tried to kick.</td>
    </tr>
    <tr>
        <td><code>samechan</code></td>
        <td>Tells the client that they are already in the channel they are trying to switch to.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>ipchan</code></td>
        <td>Tells the client that they aren't allowed to switch to the provided channel.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>nochan</code></td>
        <td>Tells the client that the channel they tried to switch to does not exist.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>nopwchan</code></td>
        <td>Tells the client that the channel they tried to switch to requires a password.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>ipwchan</code></td>
        <td>Tells the client that the password to tried to switch to the channel to was invalid.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>inchan</code></td>
        <td>Informs the client that the channel name contained invalid characters.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>nischan</code></td>
        <td>Tells the client that a channel with that name already exists.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>ndchan</code></td>
        <td>Tells the client that they're not allowed to delete that channel.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>namchan</code></td>
        <td>Tells the client that they're not allowed to edit that channel.</td>
        <td>The name of the channel.</td>
    </tr>
    <tr>
        <td><code>nameinuse</code></td>
        <td>Tells the client that the nickname they tried to use is already being used by someone else.</td>
        <td>The name.</td>
    </tr>
    <tr>
        <td><code>rankerr</code></td>
        <td>Tells the client that they're not allowed to do something to a user because they have a higher hierarchy than them.</td>
        <td></td>
    </tr>
</table>

## Commands
Actions sent through messages prefixed with `/`. Arguments are described as `[name]`, optional arguments as `[name?]`.

<table>
    <tr>
        <th>Name</th>
        <th>Action</th>
        <th>Expected bot messages</th>
    </tr>
    <tr>
        <td><code>/afk [reason?]</code></td>
        <td>Marks the current user as afk, the first 5 characters from the user string are prefixed uppercase to the current username prefixed by <code>&amp;lt;</code> and suffixed by <code>&amp;gt;_</code> resulting in a username that looks like <code>&lt;AWAY&gt;_flash</code> if <code>/afk away</code> is ran by the user <code>flash</code>. If no reason is specified "<code>AFK</code>" is used.</td>
        <td></td>
    </tr>
    <tr>
        <td><code>/nick [name?]</code></td>
        <td>Temporarily changes the user's nickname prefixed with <code>~</code>. If the user's original name or no argument at all is specified the command returns the user's name to its original state without the prefix.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to change own nickname.</li>
                <li><code>nameinuse</code>: Someone else is using the username.</li>
                <li><code>nick</code>: Username has changed.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/msg [username] [message]</code><br>
            <code>/whisper [username] [message]</code>
        </td>
        <td>Sends a private message to another user.</td>
        <td>
            <ul>
                <li><code>cmderr</code>: Missing username and/or message arguments.</li>
                <li><code>usernf</code>: User not found.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/me [message]</code><br>
            <code>/action [message]</code>
        </td>
        <td>Sends a message but with flags <code>11000</code> instead of the regular <code>10010</code>, used to describe an action.</td>
        <td></td>
    </tr>
    <tr>
        <td>
            <code>/who [channel?]</code>
        </td>
        <td>If no argument is specified it'll return all users on the server, if a channel is specified it'll return all users in that channel.</td>
        <td>
            <ul>
                <li><code>nochan</code>: The given channel does not exist.</li>
                <li><code>whoerr</code>: The user does not have access to the channel.</li>
                <li><code>whochan</code>: Listing of users in a channel.</li>
                <li><code>who</code>: Listing of users.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/delete [channel name or message id]</code>
        </td>
        <td>If the argument is entirely numeric this function acts as an alias for <code>/delmsg</code>, otherwise <code>/delchan</code>.</td>
        <td></td>
    </tr>
    <tr>
        <td>
            <code>/join [channel] [password?]</code>
        </td>
        <td>Switches the current user to a different channel.</td>
        <td>
            <ul>
                <li><code>nochan</code>: The given channel does not exist.</li>
                <li><code>ipchan</code>: The user does not have access to the channel.</li>
                <li><code>ipwchan</code>: The provided password was invalid.</li>
                <li><code>nopwchan</code>: A password is required to enter the given channel.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/create [hierarchy?] [name]</code>
        </td>
        <td>Creates a new channel.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to create channels.</li>
                <li><code>cmderr</code>: Command isn't formatted correctly.</li>
                <li><code>rankerr</code>: Tried to set channel hierarchy higher than own.</li>
                <li><code>inchan</code>: Given name contained invalid characters.</li>
                <li><code>nischan</code>: A channel with the given name already exists.</li>
                <li><code>crchan</code>: Successfully created channel.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/delchan [name]</code>
        </td>
        <td>Deletes an existing channel.</td>
        <td>
            <ul>
                <li><code>cmderr</code>: Command isn't formatted correctly.</li>
                <li><code>nochan</code>: No channel with the given name exists.</li>
                <li><code>ndchan</code>: Not allowed to delete this channel.</li>
                <li><code>delchan</code>: Deleted channel.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/password [password?]</code><br>
            <code>/pwd [password?]</code>
        </td>
        <td>Changes the password for a channel. Specify no argument to remove the password.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to change the password.</li>
                <li><code>cpwdchan</code>: Success.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/privilege [hierarchy]</code><br>
            <code>/rank [hierarchy]</code><br>
            <code>/priv [hierarchy]</code>
        </td>
        <td>Changes what user hierarchy is required to enter a channel.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to change hierarchy.</li>
                <li><code>rankerr</code>: Missing rank argument or trying to set hierarchy to one higher than their own.</li>
                <li><code>cprivchan</code>: Success.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/say [message]</code>
        </td>
        <td>Broadcasts a message as the server to all users in all channels.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to broadcast.</li>
                <li><code>say</code>: Broadcast message.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/delmsg [message id]</code>
        </td>
        <td>Deletes a message.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to delete messages.</li>
                <li><code>cmderr</code>: Invalid arguments.</li>
                <li><code>delerr</code>: The message does not exist, or the user's hierarchy is lower than the sender.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/kick [user] [time?]</code>
        </td>
        <td>Kicks a user from the server. If no time is specified the kick expires immediately.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to kick users.</li>
                <li><code>usernf</code>: User not found.</li>
                <li><code>kickna</code>: Sender is trying to kick themselves, someone with a higher hierarchy or someone that's already banned.</li>
                <li><code>cmderr</code>: Provided time is invalid.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/ban [user] [time?]</code>
        </td>
        <td>Kicks a user and IP address from the server. If no time is specified the kick will never expire.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to kick users.</li>
                <li><code>usernf</code>: User not found.</li>
                <li><code>kickna</code>: Sender is trying to kick themselves, someone with a higher hierarchy or someone that's already banned.</li>
                <li><code>cmderr</code>: Provided time is invalid.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/unban [user]</code><br>
            <code>/pardon [user]</code>
        </td>
        <td>Revokes the ban of a user.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to revoke user bans.</li>
                <li><code>notban</code>: User is not banned.</li>
                <li><code>unban</code>: Success.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/unbanip [user]</code><br>
            <code>/pardonip [user]</code>
        </td>
        <td>Revokes the ban of an ip address.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to revoke ip bans.</li>
                <li><code>notban</code>: IP address is not banned.</li>
                <li><code>unban</code>: Success.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/bans</code><br>
            <code>/banned</code>
        </td>
        <td>Retrieves the list of banned users and ip addresses.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to revoke ip bans.</li>
                <li><code>banlist</code>: List of bans.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/silence [username] [time?]</code>
        </td>
        <td>Silences a user. If the time argument is not specified the silence is indefinite.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to silence users.</li>
                <li><code>usernf</code>: User not found.</li>
                <li><code>silself</code>: Tried to silence self.</li>
                <li><code>silperr</code>: Tried to silence user of higher hierarchy.</li>
                <li><code>silerr</code>: User is already silenced.</li>
                <li><code>cmderr</code>: Time isn't formatted correctly.</li>
                <li><code>silence</code>: Informs the user they have been silenced.</li>
                <li><code>silok</code>: Tells the sender that the user has been silenced.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/unsilence [username]</code>
        </td>
        <td>Revokes a silence.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to revoke silences.</li>
                <li><code>usernf</code>: User not found.</li>
                <li><code>usilperr</code>: Tried to revoke silence of user of higher hierarchy.</li>
                <li><code>usilerr</code>: User isn't silenced.</li>
                <li><code>unsil</code>: Informs the user that their silence has been revoked.</li>
                <li><code>usilok</code>: Tells the sender that the user's silence has been revoked.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td>
            <code>/ip [username]</code><br>
            <code>/whois [username]</code>
        </td>
        <td>Gets a user's IP address.</td>
        <td>
            <ul>
                <li><code>cmdna</code>: Not allowed to view IP addresses.</li>
                <li><code>usernf</code>: User not found.</li>
                <li><code>ipaddr</code>: IP address of user.</li>
            </ul>
        </td>
    </tr>
</table>
