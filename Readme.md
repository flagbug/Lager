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

## Android

Since the Android preferences API is a nightmare to use, Lager has a useful function called `BindToSetting`.
`BindToSetting` is an extension method for the `Preference` class and creates a two-way binding between the `Preference` and a property in a `SettingsStorage`.
Changes to the `Preference` are automatically propagated to the `SettingsStorage` and vice-versa.

An example:

Define you settings layout like normal, but without default values or any other nonsense

	<PreferenceScreen xmlns:android="http://schemas.android.com/apk/res/android">
	  <EditTextPreference
		  android:key="pref_text"
		  android:summary="Save text here"
		  android:title="Text" />
	</PreferenceScreen>

And in your settings activity you simply write:

	protected override void OnCreate(Bundle bundle)
	{
		// Layout setup and stuff
		
		var textPreference = (EditTextPreference)this.FindPreference("pref_text");
		textPreference.BindToSetting(UserSettings.Instance, x => x.MySettingsString, x => x.Text, x => x.ToString());
	}
	
So what have we done here?

As said before, `BindToSetting` is an extension method for the `Preference` class, so we pulled an instance of our `EditTextPreference` from our `PreferenceActivity`.

The first parameter is an instance of our settings storage. For simplification, we just assume a singleton instance here.

The second parameter is an expression that describes what property in our settings storage we want to bind.

The third parameter is an expression that describes on which property of or `Preference` we want our setting to be bound on.
Here we have an `EditTextPreference`, so we bind it the its `Text` property. If you have a `CheckBoxPreference` for example, you most likely want to bind on its `Checked` property.

The fourth parameter is a function that converts a value from the `Preference` class to our `SettingsStorage`. 
This is a necessary step, because we have no type info when the `Preference` notifies us that the user entered a value.

There is also an optional fifth and sixth parameter.
The fifth parameter allows for a conversion from the setting in your `SettingsStorage` to the `Preference`s property.
This is useful if you want to bind non-string types to the `Text` property of an `EditTextPreference` or an enum to a `ListPreference`.

With the sixth parameter you can validate user input. It takes a function that returns a `bool` and provides the value that the user has entered.
Return true, and the value will be saved, return false and the value will be discarded.