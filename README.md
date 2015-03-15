MarkupDiff
==========
MarkupDiff is a utility for comparing markup files to detect differences. It is intended for web developement where similar markup in different contexts needs to be manually synchronized without the aid of automated code merging.

How it works
------------
- You have some markup in a source project. You copy that markup to files in a receiving project, change content, but want to maintain your markup structure. You can add a comment in your a recipient file with the file name its markup came from, and MarkupDiff will find the source file and compare the markup.

Setup
------
In your MarkupDiff.exe folder, in /projects
- Create an empty xml fil, and add the following to it :
	<project 
		sourceFolder="" 
		destinationFolder="" 
		matchTag="@*src{file}:*@" 
		sourceFileTypes="hbs" 
		destinationFileTypes="cshtml"  
	/>

- *sourceFolder* : where you source markup files are stored.
- *destinationFolder* : is where your destination files are stored.
- *matchTag* : tag in the target file with the name of the source file. Multiple tags can be added to a file if its markup comes from several files. A tag can be placed anywhere in a file, comparison is done from after its position.
- *sourceFileTypes* : comma-separated list of file types in source project to anayse
- *destinationFileTypes* : Same as *sourceFileTypes*, but in targer project.

You can have multiple project files in the /project folder.

Notes
-----
MarkupDiff is not an editor - it reads only.

Known quirks
------------
- File viewer flickers when files are edited.

Status
------
This is still a work in progress.
- the actual diff comparison is still being tweaked
- there is no project editor yet, you need to config MarkupDiff manually.

Build
-----
You can build MarkupDiff yourself, just add MsBuild to your Windows path.
- run /build/build.bat
- build output is in /_build

