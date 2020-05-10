# Custom Windows Properties

A stand-alone application for creating, installing and uninstalling custom Windows properties.

It looks like this:

![](Screenshot.png)

What it does:
- Keep a catalog of custom properties, which are presented in the tree in the top left panel of the screen. This catalog is kept in a data folder. The very first thing you will need to do on opening Custom Windows Properties for the first time is to use the 'Choose Save Folder...' button to say what folder should be used.
- The top middle panel is the property editor. When Custom Windows Properties opens, the editor contains a new property with default field values. If at a later stage, you want to create another new property, press the New button.
- When an individual field is being edited, help text about that field is shown in the top right panel.
- Once you have started making changes in the editor, a summary of the changes that you have made appears in the differences panel in the bottom right of the screen.
- If at any point you want to discard the changes that you have made, press the Discard button.
- Once you are happy with your changes, save the property by pressing the Save button. This will either update an existing saved property, if one exists with that name, or create a new saved property.
- Once you have saved a property, it will appear in the saved tree in the top left panel.
- Selecting any property in the saved tree will load it into the editor. Any changes you make to the saved values will be shown in the differences panel, as before.
- You can only delete a property that has been saved. To do so, select it in the saved tree, right click on it to show the context menu, and choose Delete.
- The installed tree in the bottom left panel shows all of the properties installed on the system, including pre-installed and custom properties.
- When an installed property is selected, all of its properties that are made available by the system are shown in the bottom middle panel.
- If an installed property has characteristics that you want in your custom property, use the Copy button to copy its field values to the editing area.
- When you are ready to install a custom property that you have created, there are two options. If the property has been saved, select it in the saved tree, then use the Install command in its context menu. Alternatively, if the property is currently loaded in the editor, click the Install button. This will save the property and then install it. If the installation succeeds, your property will now appear in the installed properties tree at the bottom left. The property will also be read back from the system, and the values obtained shown in the middle bottom panel. Installed properties are also shown with a green background in the saved tree.
- To uninstall a custom property from the system, select it in the installed property tree, and then use the Uninstall command in its context menu. You can only uninstall a property which also appears in the saved tree, which basically means properties that you yourself have created. Accordingly, you cannot delete a property which has been installed: you must uninstall it first.
- The thicker bars between the 'Saved:', 'Editor:' and 'Help:' titles above the top three panels are resize handles that you can use to change the window layout.
- By default, the differences made by changes in the editor will be shown relative to whatever set of properties were last loaded into it (for instance, when a property is selected in the saved tree, or the copy button is used to load the editor from an installed property). You can use the radio buttons above the differences panel to show the differences to the most recently seen saved or installed property instead. For instance, after you have installed a property, click on the Installed radio button to see the differences between the property you created and the property now installed on the system.
- If you want to use your custom properties as part of an installation package, in the custom property tree, select either an individual property, or the immediate parent of a set of properties, and use the Export command in its context menu. This will create a .propdesc file in the data folder that contains the single property, or the set of child properties of the chosen parent.

## Usage Notes
- In the Microsoft documentation about creating custom properties, the recommendation is to think hard about what you need before you start, and not to chop and change too much. One good reason for this is that for searchable properties, which is most useful ones, the Index Server builds indexes, and gets confused if things change too much. The warning is that you may finish up needing to rebuild the index.
- The best way to think about custom properties is that each one is a separate idea, with its own unique name and identity. Custom properties may share parts of their name, for example Sample.Location.AreaCode and Sample.Location.Rating. But this is of no great significance to the operating system. 
- Multilevel property names help you to group and find them. A good way to think about this is by analogy with files and folders. They have similar restrictions. So, for example, once you have created a property called Sample.City, you cannot create another one called Sample.City.Population. Equally, if you have created a property called Sample.City.Population, you cannot create another one called Sample.City, but you could create a property called Sample.Country, or one called Sample.City.Area.
- One difference to files and folders, however, is that you cannot rename a property or a folder. You can clone a property and create one with a new name, or you can delete it, and that is all. And you certainly cannot rename the equivalent of a 'folder'.
- There is an important distinction between what you see in the editor and the saved tree, and what you see in the installed panels. The installed panels show only data that has been read from the operating system. The editor and the saved tree show the configuration that you hope will be achieved should you install it.
- It is not actually possible to obtain from the operating system values for all of the property attributes that you can configure. Installed property information shown in the panel at the middle bottom of the screen mark such attributes as 'unavailable'.
- The main focus of Custom Windows Properties is on attributes that can be round-tripped, i.e. those that can be configured in an installation and then read back to confirm their values. Custom Windows Properties covers nearly 100% of such attributes (an example of a holdout is RelativeDescriptionType, which is used to control how an API which compares two property values presents its results, and therefore of little use unless you write a program that uses the API).
- Some attributes are editable, even though they cannot be read back, if they are of particular significance. Examples are the EditControl, and the display formatting attributes. However, coverage is less complete in these areas, perhaps at 75%. Requests for enhancements will be considered!
- When you first run Custom Windows Properties, about the first thing that you need to do is to specify the Saved Data folder. This is where Custom Windows Properties keeps all of the saved properties, and if you copy the contents of this directory from machine to machine, and specify the copied folder as the Saved Data folder on the target machine, the saved properties will appear in the saved tree in Custom Windows Properties on the target machine.
- However, installed property configurations need to be held in a separate area, partly to ensure that they are accessible to all users of the machine, and partly because Windows may load data from them lazily at any time, and so they had better not move about after installation. The folder used is <System drive>:\ProgramData\CustomWindowsProperties. It is strongly recommended that you leave the contents of this folder alone. It is not useful to copy this folder from machine to machine: instead, copy the Saved Data folder and install the properties on the new machine.

## Versions:   
0.6 - Release candidate 3

## Work list
- Source all messages from localised resources
- Create a commandline tool to install and uninstall propdesc files
- Document technical detail of what parts of the schema are not covered
- Document technical detail on current best understanding of what incomplete installation might mean
