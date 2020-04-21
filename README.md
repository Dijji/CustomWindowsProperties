# Custom Windows Properties

To do Write the readme

**Latest:** Version 1.6 is now the recommended [release](../../releases/tag/v1.6). All [documentation](../../wiki) has been updated to reflect the changes, which are described fully in the release notes. The most important change is a revision to how
File Meta works when a handler is already present, to allow for the fact that Windows 10 does not allow some of its built-in handlers to be substituted. I recommend reading [Using the File Meta Association Manager](../../wiki/Using-the-File-Meta-Association-Manager) to make sure you understand exactly what is going to happen when a handler is already present, and how to make it work for you.

In Windows XP, Explorer could see and edit metadata (for example comments or tags) for any type of file. In Vista and later, this has been possible only for certain types of file, such as Office documents, JPEGs and MP3s. 

It is pretty clear that Microsoft originally intended to ship a broader capability. What this package does is wire together pieces that were built into Windows in readiness, but never joined up: it connects Explorer's ability to see and edit metadata with NTFS's support for storing property data in an annex to any file, and so allowing metadata to be added to files of any type.  And because Windows Search uses the same property system hooks as Explorer, you can also search using this metadata, both in Explorer and from the Start Menu (or Search charm).  That all this takes just a 19K DLL (64-bit, release build) and some registry settings tells you how close Microsoft got. 

One reason why Microsoft never shipped the complete solution is that all works well when the file is moved around between NTFS drives, but the metadata is lost when a file is, say, emailed, or moved to a FAT file system.  Also, if your files are still being edited, then you need to check that associated metadata is preserved on save.  Some file editors, rather than update the file in place, save updates using a write-new-file, delete-original-file, rename-new-to-original strategy that loses any metadata held in the annex to the original file.  You do need to be aware of these limitations, but think of being able to add metadata to txt files, pdfs, anything, editable directly in Explorer!  And because XP used the same storage mechanism for the general case (see [XP, Vista and File Metadata](../../wiki/XP,-Vista-and-File-Metadata) for the full story), this package will also read some metadata written under XP, hidden by a Windows upgrade, and invisible ever since .  Also, to help with unfriendly editors and non-NTFS transfers, the installation optionally sets up an Explorer context menu that lets you right click on a file to export its metadata to a separate XML file, and import it again to reapply it to the original file (or indeed to a different file).  If you want to backup all your metadata, then there is also a command line utility for bulk metadata export and import that works with the same XML file format.

The final element of the package is a file association manager, to manage the registry settings that turn file metadata support on (and off) per file extension and tell Explorer to allow you to see and edit the metadata for a file type.  By default, file metadata will not appear for any additional extensions, you need to turn support on explicitly per extension. To protect existing functionality, the file association manager will not let you interfere where Explorer already knows how to handle a file type, it only allows you to add metadata support for file types that aren't already covered by Windows or 3rd party software.  

To see how File Metadata appears in Windows Explorer, look at [What you see](../../wiki/What-you-see).  For the required installation steps, see [Getting Started](../../wiki/Getting-Started).    Once you've had a go, any feedback, positive or negative, would be much appreciated.

Here is the [Documentation](../../wiki), but if you have any problems or would like to raise a question, please open an Issue.



