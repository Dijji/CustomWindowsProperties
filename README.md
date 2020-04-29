# Custom Windows Properties

A stand-alone application for creating, installing and uninstalling custom Windows properties.

It looks like this:

![](Screenshot.png)

What it does:
- Keep a catalog of custom properties, which are presented in the tree in the top left of the screen. This catalog is kept in a data folder. The very first thing you will need to do on opening the app for the first time is to use the 'Choose Save Folder...' button to say what folder the app should use.
- When a custom property is selected, show all of its properties in the top middle panel, and allow them to be edited and saved.
- When an individual field is being edited, help text about that field is shown in the top right panel.
- A summary of the changes that you have made appear in the differences panel in the bottom right of the screen.
- If at any point you want to discard the changes that you have made, press the Discard button.
- Create a new custom property by entering its new name and saving it.
- Once you have saved a property, it will appear in the tree at the top left panel.
- Selecting any property in the saved tree will load it into the editor. Any changes you make to the saved values will be shown in the differences panel, as before.
- You can only delete a property that has been saved. To do so, select it in the saved tree, right click on it to show the context menu, and choose Delete.
- The installed tree in the bottom left panel shows all of the properties installed on the system, including pre-installed and custom properties.
- When an installed property is selected, show all of its properties that are made available by the system in the bottom middle panel.
- If an installed property has characteristics that you want in your custom property, use the Copy button to copy them to the editing area.
- When you are ready to install a custom property that you have created, there are two options. If the property has been saved, selected in the saved tree, then use the Install command in its context menu. Alternatively, if the property is currently loaded in the editor, click the Install button. This will save the property and then install it. If the installation succeeds, your property will now appear in the installed properties tree at the bottom left. The property will also be read back from the system, and the values obtained shown in the middle bottom panel.
- To uninstall a custom property from the system, select it in the installed property tree, and then use the Uninstall command in its context menu. You can only uninstall a property which also appears in the saved tree, which basically means properties that you yourself have created. Accordingly, you cannot delete a property which has been installed: you must uninstall it first.
- By default, the differences made by changes in the editor will be shown the relative to whatever set of properties were last loaded into it (for instance, when a property is selected in the saved tree, or the copy button is used to load the editor from an installed property). You can use the radio buttons above the differences panel to show the differences to the most recently seen saved or installed property instead.
- If you want to use your custom properties as part of an installation package, in the custom property tree, select either an individual property, or the immediate parent of a set of properties, and use the Export command in its context menu. This will create a .propdesc file in the data folder containing the single property, or the set of child properties of the chosen parent.

## Versions:   
0.1 - First working version, for testing functionality and usability

## Work list
- Write the readme!
- Go through properties and see which can be sourced from the system, but are not at the moment, and those which could not be but are not marked as unavailable.
- See if the registry will point out system property descriptions that we can parse for some of the missing information
- Add the help text for the properties, from a localised resource
- Source all messages from localised resources
- Look to see if any more display properties should be added
- If two properties are added, with names differing only in the last part, then they should have the same format ID, but different property IDs
- Consider remembering tree selections and restoring them or bringing them into view after the tree is updated, where required
- Document this subtlety for canonical name: if A.B.C exists, then A.B is not an acceptable name for a new terminal
- Document how names work independently, no such thing as folder renaming
- Document the separate permanent universally available location for the propdesc files that we install. The system loads from them lazily, possibly using the identity of whoever is signed on, or the system, in the case of the index server.
- Document current best understanding of what incomplete installation might mean

Example markup:
**Latest:** Version 1.6 is now the recommended [release](../../releases/tag/v1.6).  [documentation](../../wiki) has been updated 
