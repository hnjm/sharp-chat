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
        <td>32-bit Unix timestamp</td>
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
        <td>Any amount of data required to complete authentication.</td>
    </tr>
</table>

### Packet `2`: Message
Informs the server that the user has sent a message.

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
        <td>Message text, additional packet parameters should be glued on the server using <code>\t</code>.</td>
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
        <td>Message text, additional packet parameters should be glued on the server using <code>\t</code>.</td>
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
        <td>32-bit Unix timestamp when the packet was handled by the server.</td>
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

#### Successful authentication response
Informs the client that authentication has succeeded.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>y</code> for yes.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Session User ID.</td>
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
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Default channel the user will join following this packet.</td>
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
        <td>Literal string <code>n</code> for no.</td>
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
        <td>If <code>baka</code>; A 32-bit Unix timestamp indicating the length of the ban.</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Literal string <code>n</code> for no.</td>
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
        <td>If <code>joinfail</code>; A 32-bit Unix timestamp indicating the length of the ban.</td>
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
        <td>32-bit Unix timestamp of when the user joined.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username.</td>
    </tr>
    <tr>
        <td><code>color (int)</code></td>
        <td>Username color in packed format, documented below.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>32-bit Unix timestamp of when the user joined.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
    </tr>
</table>

### Packet `2`: Chat message
Informs the client that a chat message has been received.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>32-bit Unix timestamp of when the user joined.</td>
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
            <p>Message (sanitised on the server).</p>
            <p>
                If this is an informational message this field is formatted as follows and concatenated by the form feed character <code>\f</code>, respresented in ASCII by <code>0x0C</code>.
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
                        <td>
                            An id of a string in the legacy language files.
                            A list can be found in the <code>botText</code> and <code>botErrText</code> sections of <a href="https://sockchat.flashii.net/legacy/lang/en/common.json">sockchat.flashii.net/legacy/lang/en/common.json</a> and <a href="https://sockchat.flashii.net/legacy/lang/en/core.json">sockchat.flashii.net/legacy/lang/en/common.json</a>.
                        </td>
                    </tr>
                    <tr>
                        <td><code>...string</code></td>
                        <td>
                            Any number of parameters used to format the language string.
                        </td>
                    </tr>
                </table>
            </p>
        </td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
    </tr>
    <tr>
        <td><code>message flags (string)</code></td>
        <td>Message flags, documented below.</td>
    </tr>
</table>

### Packet `3`: User disconnect
Informs the client that a user has disconnected.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username.</td>
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
        <td>32-bit Unix timestamp of when the user disconnected.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
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
        <td>The name of the new channel.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs.</td>
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
        <td>The old/current name of the channel.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>The new name of the channel.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is password protected.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs.</td>
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
        <td>The name of the channel that has been deleted.</td>
    </tr>
</table>

### Packet `5`: Channel switching
This packet informs the client about channel switching.

**DEPRECATED IN VERSION 2**: This packet is specific to the single channel instance nature of Version 1 and thus goes unused.

**Consideration**: 5.2 could still be useful for forcing a user to enter a channel.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>
            Channel information update event ID.
            <ul>
                <li><code>0</code>: A user joined the channel (again). Sent to all users in a channel.</li>
                <li><code>1</code>: A user left the channel. Sent to all users in a channel.</li>
                <li><code>2</code>: The client is to forcibly switch to a different channel.</li>
            </ul>
        </td>
    </tr>
</table>

#### Sub-packet `0`: Channel join
Informs the client that a user has joined the channel.

<table>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Username.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>CSS username color.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
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
        <td>User ID.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID.</td>
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
        <td>The name of the channel that the user has been switched to.</td>
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
        <td>Event ID of the deleted message.</td>
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
        <td>Amount of users present in the channel.</td>
    </tr>
    <tr>
        <td><code>Context User</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below.</td>
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
        <td><code>color (int)</code></td>
        <td>Username color in packed format, documented below.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list.</td>
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
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the user should be visible in the users list.</td>
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
        <td>32-bit Unix timestamp</td>
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
        <td><code>color (int)</code></td>
        <td>Username color in packed format, documented below.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message.</td>
    </tr>
    <tr>
        <td><code>message flags (string)</code></td>
        <td>Message flags, documented below.</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>32-bit Unix timestamp</td>
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
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>Message text, functions the same as described in Packet <code>2</code>: Chat message</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>Event ID</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Whether the client should notify the user about this message.</td>
    </tr>
    <tr>
        <td><code>message flags (string)</code></td>
        <td>Message flags, documented below.</td>
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
        <td>Amount of channels on the channel.</td>
    </tr>
    <tr>
        <td><code>Context Channel</code></td>
        <td>An amount of repetitions of this object based on the number in the previous param, the object is described below.</td>
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
        <td>Indicates whether the channel is password protected.</td>
    </tr>
    <tr>
        <td><code>bool</code></td>
        <td>Indicates whether the channel is temporary, meaning the channel will be deleted after the last person departs.</td>
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
                <li><code>1</code>: The client has been banned, can reconnect after the timestamp in the next param has expired.</li>
            </ul>
        </td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>A 32-bit Unix timestamp containing the ban expiration date and time.</td>
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
        <td>User ID of the affected user.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username.</td>
    </tr>
    <tr>
        <td><code>color (int)</code></td>
        <td>Username color in packed format, documented below.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
    </tr>
    <tr>
        <th colspan="2">Version 1</th>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>User ID of the affected user.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New username.</td>
    </tr>
    <tr>
        <td><code>string</code></td>
        <td>New CSS username color.</td>
    </tr>
    <tr>
        <td><code>permissions (string)</code></td>
        <td>User permissions, documented below.</td>
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
        <td>Indicates whether the version upgrade was successful.</td>
    </tr>
    <tr>
        <td><code>int</code></td>
        <td>If successful the current version will be returned. If unsuccessful the latest supported version should be returned which the client could use to decide to either disconnect or reattempt to upgrade.</td>
    </tr>
</table>

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

    char* css = (char*)malloc(17);
    memset(css, 0, 17);

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
