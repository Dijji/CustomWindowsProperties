# Custom Windows Properties

A stand-alone application for creating, installing and uninstalling custom Windows properties.

Versions:   
0.1 - First working version, for testing functionality and usability

## Work list
- Write the readme!
- Go through properties and see which can be sourced from the system, but are not at the moment, and those which could not be but are not marked as unavailable.
- Add the help text for the properties, from a localised resource
- Source all messages from localised resources
- Look to see if any more display properties should be added
- Extend error checking for canonical name: if A.B.C exists, then A.B is not an acceptable name
- Document how names work independently, no such thing as folder renaming
- If two properties are added, with names differing only in the last part, then they should have the same format ID, but different property IDs
- Detect when the edited property is dirty, so that we can warn about changes being lost as appropriate

Example markup:
**Latest:** Version 1.6 is now the recommended [release](../../releases/tag/v1.6).  [documentation](../../wiki) has been updated 
