====================== Package Info =========================

name: InfiniWheel Wheel Picker						

author: Hence Games -> (Wijnand van Tol & Ies Wierdsma)

email: InfiniWheel@HenceGames.com

version: v1.1

=============================================================

Thanks for purchasing our package and giving your users a smooth way of selecting items from a list!

The InfiniWheel wheel picker is a nice and easy to implement package that allows you to put wheel pickers in your UI in just a matter of seconds.
It uses the new and improved Unity 4.6 UI, making it easy to implement, because it doesn't require other packages. 
The wheel can be fully customized to your liking too!

Included in this package are:
- An InfiniWheel prefab & script
- A sample scene
- Example code that uses InfiniWheel for a date and time picker.
- As a bonus, a nice UI material shader!

To use InfiniWheel, just import the package into your project and drag the prefabs into your scene. 
To initialize the wheel, you have to call Init (params string[] choices) in code, with the values that you want on the wheel picker.
To set the wheel to a specific index, call Select(int index).
Everytime the value of a wheel changes, a ValueChange event is fired, which you can use in your game/application.

To customize the wheel, adjust the images on
	Wheel 				(the background)
	Wheelitem 			(the background of the item itself)
	Gradient Overlay 	(the 3d effect)
	Selector 			(the (default green) bar that hovers over the center)

You can use our sample scene as an example how to interact with InfiniWheel. It also includes a free script for a date and time picker!