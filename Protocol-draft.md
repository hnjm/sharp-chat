# Sock Chat Protocol Information
The protocol operates on a websocket in text mode. Messages sent between the client and server are a series of concatenated strings delimited by the vertical tab character, represented in most languages by the escape sequence `\t` and defined in ASCII as `0x09`.
The first string in this concatenation must be the packet identifier, sent as an integer. The packet identifiers are as follows.

Some instructions are specific to newer revisions of the protocol and some instructions behave differently in newer revisions, all versions are documented but it is recommended you use the latest one. If a packet is marked as deprecated and you only aim to implement the latest version, you may omit it in your implementation as it will never be sent.

The current stable version of the protocol is **Version 1**.

## Client
These are the packets sent from the client to the server.

### Packet `0`: Ping
Used to prevent the client from closing the session due to inactivity.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
</table>

### Packet `1`: Authentication
Takes a variable number of parameters that are fed into the authentication script associated with the chat.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>...any</code></td>
        <td>Any amount of data required to complete authentication</td>
    </tr>
</table>

### Packet `2`: Message
Informs the server that the user has sent a message.

Required commands for Version 1 are described lower in the document.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Channel name</td>
    </tr>
    <tr>
        <td><code>...string</code></td>
        <td>Message text, additional packet parameters should be glued on the server using <code>\t</code></td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>...string</code></td>
        <td>Message text, additional packet parameters should be glued on the server using <code>\t</code></td>
    </tr>
</table>

### Packet `3`: Upgrade
Informs the server that this client supports a newer version of the protocol. This should always be the first thing you send if you want to upgrade the connection, if any other packet is sent beforehand, this one will be ignored and the session will operate in version 1 mode. An upgrade packet containing any version below 2 should be ignored.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Version number</td>
    </tr>
</table>

### Packet `4`: Typing
Informs the server that this client is writing a message.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Channel name</td>
    </tr>
</table>

## Server
These are the packets sent from the server to the client.

### Packet `0`: Pong
Response to client packet `0`: Ping.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below when the packet was handled by the server</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The literal string <code>pong</code></td>
    </tr>
</table>

### Packet `1`: Join/Auth
While authenticated this packet indicates that a new user has joined the server/channel. Before authentication this packet serves as a response to client packet `1`: Authentication.

This packet behaves differently between version 1 and 2: In version 1 this packet is only sent to the channel that the user is about to join, in version 2 this packet is sent server wide.

#### Successful authentication response
Informs the client that authentication has succeeded.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>y</code> for yes</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Session User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color in packed format, documented below</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>y</code> for yes</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Session User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Default channel the user will join following this packet</td>
    </tr>
</table>

#### Failed authentication response
Informs the client that authentication has failed.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>n</code> for no</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>
            Reason for failure.
            <ul>
                <li><code>auth</code>: Authentication data is invalid.</li>
                <li><code>conn</code>: User has exceeded the maximum amount of connections per user.</li>
                <li><code>baka</code>: User attempting to authenticate is banned.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>If <code>baka</code>; A timestamp (documented below) indicating the length of the ban</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>n</code> for no</td>
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
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>If <code>joinfail</code>; A timestamp (documented below) indicating the length of the ban</td>
    </tr>
</table>

#### User joining
Informs the client that a user has joined.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the user joined</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color in packed format, documented below</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the user joined</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
</table>

### Packet `2`: Chat message
Informs the client that a chat message has been received.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Target name: Channel name, <code>@broadcast</code> or <code>@log</code>.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the message was received by the server</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message, <b>NOT SANITISED</b></td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags, documented below</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the message was received by the server</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>
            User ID.
            If <code>-1</code> this message is an informational message from the server and the next field takes on a special form.
        </td>
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
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags, documented below</td>
    </tr>
</table>

### Packet `3`: User disconnect
Informs the client that a user has disconnected.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
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
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the user disconnected</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
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
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below of when the user disconnected</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
</table>

### Packet `4`: Channel event
This packet informs the user about channel related updates. The only consistent parameter across sub-packets is the first one described as follows.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
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
    </tr>
</table>

#### Sub-packet `0`: Channel creation
Informs the client that a channel has been created.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The name of the new channel</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
    </tr>
</table>

#### Sub-packet `1`: Channel update
Informs the client that details of a channel has changed.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The old/current name of the channel</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The new name of the channel</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
    </tr>
</table>

#### Sub-packet `2`: Channel deletion
Informs the client that a channel has been deleted

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The name of the channel that has been deleted</td>
    </tr>
</table>

### Packet `5`: Channel switching
This packet informs the client about channel switching.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
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
    </tr>
</table>

#### Sub-packet `0`: Channel join
Informs the client that a user has joined the channel.

In version 1 this packet is NOT sent when the user first connects to the server. In version 2 this packet is sent regardless.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
</table>

#### Sub-packet `1`: Channel departure
Informs the client that a user has left the channel.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
</table>

#### Sub-packet `2`: Forced channel switch
Informs the client that it has been forcibly switched to a different channel.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The name of the channel that the user has been switched to</td>
    </tr>
</table>

### Packet `6`: Message deletion
Informs the client that a message has been deleted.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID of the deleted message</td>
    </tr>
</table>

### Packet `7`: Context information
Informs the client about the context of a channel before the client was present.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
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
    </tr>
</table>

#### Sub-packet `0`: Existing users
Informs the client about users already present in the channel.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Amount of users present in the channel</td>
    </tr>
    <tr>
        <td><code>Context User</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
    </tr>
</table>

##### Context User object

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color in packed format, documented below</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list</td>
    </tr>
</table>

#### Sub-packet `1`: Existing message
Informs the client about an existing message in a channel.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color in packed format, documented below</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message</td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags, documented below</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Timestamp, documented below</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Sequence ID</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message</td>
    </tr>
    <tr>
        <td><code>message flags</code></td>
        <td>Message flags, documented below</td>
    </tr>
</table>

#### Sub-packet `2`: Channels
Informs the client about the channels on the server.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Amount of channels on the channel</td>
    </tr>
    <tr>
        <td><code>Context Channel</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below</td>
    </tr>
</table>

##### Context Channel object

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Channel name</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs</td>
    </tr>
</table>

### Packet `8`: Context clearing
Informs the client that the context has been cleared.

**DEPRECATED IN VERSION 2**

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
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
    </tr>
</table>

### Packet `9`: Forced disconnect
Informs the client that they have either been banned or kicked from the server.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>
            <ul>
                <li><code>0</code>: The client has been kicked, can reconnect immediately.</li>
                <li><code>1</code>: The client has been banned, can reconnect after the timestamp (documented below) in the next param has expired.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>A timestamp (documented below) containing the ban expiration date and time</td>
    </tr>
</table>

### Packet `10`: User update
Informs that another user's details have been updated.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the affected user</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username</td>
    </tr>
    <tr>
        <td><code>color</code></td>
        <td>Username color in packed format, documented below</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates a silent name update.</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the affected user</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New CSS username color</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below</td>
    </tr>
</table>

### Packet `11`: Upgrade acknowledgement
Responds to the client about its upgrade request through Packet `3`: Upgrade.

The client must continue to operate as if it's talking to a Version 1 server until this packet is received. This requirement does hold Packet `1`: Authentication in a strangehold, but luckily there's no reason to alter that packet at all due to its whatever nature.

<table>
    <tr>
        <th colspan="2">Version 2</th>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the version upgrade was successful</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>If successful the current version will be returned. If unsuccessful the latest supported version should be returned which the client could use to decide to either disconnect or reattempt to upgrade</td>
    </tr>
</table>

### Packet `12`: Typing
Informs the client that one or more users are typing.

### Packet `13`: Flood protection warning
Informs the client that they might be kicked soon for flood protection. This packet has no arguments.

## Timestamps

Timestamp, documented belows in Sock Chat are seconds elapsed since a certain date. Starting with Sock Chat V2 the epoch for the timestamps is different. Under Sock Chat V1 timestamps are regular Unix Epoch timestamps where `0` is `1970-01-01 00:00:00 UTC`. Starting with Sock Chat V2 the epoch has been moved to `2019-01-01 00:00:00 UTC`. In order to convert a Sock Chat V2 timestamp to a Unix timestamp add `1546300800` to it. Sock Chat V2 will realistically never serve messages that predate 2019.

## User Permission String
The User Permission String consists out of five (5) parts concatenated by a space operator, indentified in most languages as the escape sequence <code> </code> and defined as the ASCII character `0x20`.
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
Starting with Version 2, the old message flags have been replaced with a bitset describing message attributes rather than directly describing the appearance of the username.

| Bit    | Name      | Description |
| ------ | --------- | ----------- |
| `0x01` | Action    | This message describes an action. (`/me`) |
| `0x02` | Broadcast | This message is a broadcast sent to all users in all channels. **DRAFT NOTE**: Broadcasted messages are also sent to pseudo-channel `@broadcast` so this attribute might be pointless. |
| `0x04` | Log       | This message is a log sent to only a single user. **DRAFT NOTE**: Log messages are also sent to the pseudo-channel `@log` so this attribute might be pointless as well. |
| `0x08` | Private   | This message is privately sent directly from one use to another. |

### Message Flags in Version 1
The Message Flags alter how a message should be displayed to the client, these are all boolean values.
I'm not entirely sure if these allowed for custom flags, but much like the custom flags in the User Permission String, these have gone unused and should thus, also, be avoided.
The parts are as follows:

- Username should appear using a **bold** font.
- Username should appear using a *cursive* font.
- Username should appear __underlined__.
- A colon `:` should be displayed between the username and the message.
- The message was sent privately, directly to the current user.

As an example, the most common message flagset is `10010`.

## Packed color format
Starting with Version 2 colors are no longer sent as textual CSS colors, rather they're sent as a more easy to work with integer format.

The format is pretty simple and comes in the form of a signed 32-bit integer in the following format: `0xFFRRGGBB`.
`FF` is reserved for flags, although the highest bit goes unused as to avoid clashing with the sign bit.
`RR` is the byte containing the red color value, `GG` contains green and `BB` contains blue.

The only flag thusfar is `0x40` for indicating that the parent color should be inherited and the included color bytes should be ignored.

Here's some C code showing an example of converting the integer color to a CSS color string;
```
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define FLAG_INHERIT (0x40000000)

char* color_to_css(int raw) {
    if(raw & FLAG_INHERIT)
        return "inherit";

    char* css = malloc(17);

    sprintf(
        css,
        "rgb(%d,%d,%d)",
        (raw >> 16) & 0xFF,
        (raw >>  8) & 0xFF,
         raw        & 0xFF
    );

    return css;
}
```

## Bot Messages in Version 1

Formatting IDs sent by user -1 in Version 1 of the protocol.

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

## Commands in Version 1

Actions sent through messages prefixed with `/` in Version 1 of the protocol. Arguments are described as `[name]`, optional arguments as `[name?]`.

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
            <code>/join [channel]</code>
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
