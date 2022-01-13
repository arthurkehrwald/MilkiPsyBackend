# MilkiPsyBackend

This repository contains a C# console backend for testing this [Unity frontend](https://github.com/arthurkehrwald/MilkiPsyFrontend) for psychomotor training applications. It can connect to the frontend via TCP, receive status updates and send messages from a command line interface that trigger responses on the client. This code is meant to be used as a reference (not as a basis) for a more sophisticated backend capable of analysing real-time sensor data of the user to drive the learning process.

The netcode is based on [this](https://github.com/tom-weiland/tcp-udp-networking). I have no prior experience writing netcode so assume there are bugs everywhere.

## Command Line Interface

Since no automated evaluation exists at the moment, messages can only be sent manually from the command line. There are three commands:

- 'feedback' followed by the name of a JSON file on the client device displays feedback on the client
- 'popup' followed by the name of a JSON file on the client device displays a popup message on the client
- 'changestage' followed by either 'previous', 'next', or 'index' and a number changes the current stage on the client

## Packet Format

The first four bytes of both inbound and outbound messages encode the length of the message in bytes. This tells the receiver how much data to read from the stream to get the entire message. After that, various basic data types such as integers, strings, and boolean values can be added to the packet. Each of these subpackets also begins with four bytes that encode the length of the data bytes. In this example, only ASCII-encoded JSON-strings are used.

**IMPORTANT:** All sub-packets that were added to a package on one end must be read on the other end **in the same order** to avoid total mayhem.

## Inbound Message Format

Inbound messages contain information about the client state. Whenever the user selects a program or a new stage begins, a message is received. These messages provide the context necessary to evaluate user performance. They consist of a single string.

Example:
```
{
    "uniqueProgramName":"example_program",
    "uniqueStageName":"example_stage"
}
```

## Outbound Message Format

All outbound messages begin with a JSON-string containing metadata.

Example:
```
{
    "type":1,
    "currentState":
    {
        "uniqueProgramName":"example_program",
        "uniqueStageName":"example_stage"
    },
    "ignoreMessageIfOutdated":true
}
```

The first component is an enum value anncouncing what type of message follows after the metadata. The client chooses the correct parsing function based on this value.

| Value | Effect |
|-|-|
| 1 | Display feedback |
| 2 | Display popup |
| 3 | Change current stage |

Next, the most recent state information received from the client is sent back. When receiving the message, the client compares the information to its current state. If it is different, the message is outdated. In this case, the client will ignore the message if "ignoreMessageIfOutdated" is set to true.

The second string contains the essential content of the message. Three types are currently supported:

### Feedback

Feedback messages contain the name of a JSON-file on the client device that defines feedback using text and/or other media.

Example:
```
{
    "jsonFileName":"example_feedback.json"
}
```

### Popup

Messages that trigger popups also simply contain the name of a JSON-file on the client. 

Example:
```
{
    "jsonFileName":"example_popup.json"
}
```

### Change Stage

This type of message signals the client to change the current stage. The integer "function" determines how this is done.

| Value | Effect |
|-|-|
| 1 | Previous stage |
| 2 | Next stage |
| 3 | Set stage index |

If "function" is 3, another integer called "index" provides the index of the stage to switch to starting at 0 for the first stage. If the index is out of range, the client does nothing.

Example:
```
{
    "function":3,
    "index":2
}
```
