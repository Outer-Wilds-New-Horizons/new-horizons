---
Title: Config Editor
Sort_Priority: 50
---

# Config Editor

Are you tired of manually editing JSON? Do you want richer validation than a JSON schema? Well then the config editor may be for you!  

This page outlines how to install and use the config editor.

## Installation

To get started, head over to the [releases page for the editor](https://github.com/Outer-Wilds-New-Horizons/nh-config-editor/releases/latest) and install the file for your OS:

- Windows: The .msi file (not the .msi.zip and .msi.zip.sig file)
- MacOS: The .AppImage file (not the .AppImage.tar.gz or the .AppImage.tar.gz.sig file)

Follow the installer instructions to complete setup

## Creating a New Project

Creating a new project is as simple as clicking the button.  
Fill out the form with thr info for your mod and a new project will be made at the specified path.

## Editing Files

To edit a file, navigate to it in the left panel and click on it.

### JSON files

JSON files (planets, systems, etc) have a graphical interface for editing, **this will clear comments!**  

If you don't want comments to be cleared, use the text editor

#### Using the Text Editor

Already familiar with JSON and prefer text-based editing? Simply open up settings (File -> Settings) and turn on the "Always use Text Editor" option.

### Image and Audio Files

You can view images and play audio files with this editor.

### XML Files 

Right now, XML support is limited. You'll get syntax highlighting but no error checking or autofill.


## Running the Game

You can start the game from the editor by selecting Project -> Run Project this will open a new window where you can run the game

### Log Port

If you're using the mod manager and would like logs to appear there, you need to get the log port from the console, it's always the first entry in the logs. Keep in mind this port changes whenever you restart the manager.

![Get the log port]({{ "images/editor/log_port.webp"|static }})


## Building

The editor also provides a system for building your mod to a zip file, which can then be uploaded to GitHub in a release. To do this, press Project -> Build (Release)



