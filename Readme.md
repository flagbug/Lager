# Overview

Lager is an attempt to create a cross-platform settings storage for .NET

It uses [Akavache](https://github.com/akavache/Akavache) as a simple storage provider, but I'm playing with the thought of using the respective native settings storage for each platform.

Currently Lager can write and read every type of object that can be stored by Akavache.
Later versions will also include some type of migration system, for renaming and deleting settings.

# Usage

	public class UserSettings : SettingsStorage
	{
		public UserSettings() : base("D5702B73-854F-4E92-93DD-99DB026918B4", BlobCache.UserAccount)
		{ }
		
	}
	
So, what have we done here?

First, we've inherited from the `SettingsStorage` class. 
This class will provide us with our necessary methods for storing and receiving values.

Next, we've provided the `SettingsStorage` constructor with a unique string that Lager uses to avoid key-collisions.
The second parameter specifies the `IBlobCache` implementation of Akavache where our settings are stored.

Defining our settings is pretty easy too, just define properties that have the following structure:

	public string MyCoolString
	{
		get { return this.GetOrCreate("Some default value"); }
		set { this.SetOrCreate(value); }
	}
	
or

	public int MyCoolNumber
	{
		get { return this.GetOrCreate(42); }
		set { this.SetOrCreate(value); }
	}
	
*But wait, what is this magic? How does Lager know which setting it should retrieve when I call `GetOrCreate`?*

That's simple, both `GetOrCreate` and `SetOrCreate` have a second optional parameter that is marked with the `CallerMemberName` attribute.
This means a property called `MyCoolString` is stored with the key `MyCoolString`
