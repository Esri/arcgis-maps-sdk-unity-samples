# Unity C# Coding Style

The intent of this document is to provide direction and guidance to Esri Unity C# programmers that will enable them to employ good programming style and proven programming practices leading to safe, reliable, testable, and maintainable code.

## C# Coding Standards

### General Rules

- Avoid `this.` unless absolutely necessary.
- Access level modifiers must be explicitly defined for classes, methods and member variables.
- Namespace imports should be specified at the top of the file, outside of namespace declarations.
- Avoid more than one empty line.
- Avoid trailing whitespace at the end of lines.
- Do not use inline control statements.
- Do not use inline definitions.
- Use braces in control statements even in one line block cases.

	```csharp
	// Use
	if (alpha == 0)
	{
		Debug.Log("Alpha is zero");
	}
	// Instead of
	if (alpha == 0)
		Debug.Log("Alpha is zero");
	// or
	if (alpha == 0) Debug.Log("Alpha is zero");
	
	```
	
- Do not use a Hungarian notation prefix. In other words, do not use a type prefix (`float fSpeed`, `bool bIsEnabled`).
- Avoid using abbreviations. Exceptions to this rule are very obvious or standard cases `InputOutput, IO`.
- Use simple types. More info about simple types in this link: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types#simple-types.
- Use CRLF as end of line.

### Indentation and Braces

- Use Allman style (This style puts the brace on the next line to the methods header or control statement).
- Use tabs for indentation.

Example:

```csharp
foreach (var enemy in visibleEnemies)
{
	if (enemy.IsEnabled)
	{
		switch (enemy.Type)
		{
			case EnemyType.Goblin:
				break;
				
			case EnemyType.Ghost:
				break;
		}
	}	
}
```

### White Space and Blank lines

- Blank lines should be used to separate logical blocks of code in much the way a writer separates prose using headings and paragraphs.

Example:

```csharp
void function()
{
	if (something)
	{
		...
    	}
    	<blank line>
    	if (something)
    	{
        	...
    	}
}
```

### Long lines of code 

- Comments and statements that extend beyond code viewer (150 columns) in a single line can be broken up and indented for readability. When passing large numbers of parameters, it is acceptable to group related parameters on the same line.

Example:

```csharp
public void Update(
 	TileGPUData tile, 
 	Dictionary<uint, Tuple<ulong, TileLayerCPUData>> elevationLayers, 
 	Dictionary<ulong, TileGPUData> currentTiles)
 {
 	// Do Something
 }
```

### Commenting

- Single line comments should always begin with two slashes, followed by a space.
- Avoid long comments to explain methods or algorithm, the code should be self-explained. Create a separated document to explain something if it's necessary.
- No commented code.

Example:

```csharp 
// Comment
```

### Naming

#### Capitalization

- Pascal: the first letter of an identifier is capitalized as well as the first letter of each concatenated word.
- Camel: the first letter of an identifier is lowercase but the first letter of each concatenated word is capitalized.

<table>
<tr><td><b>Identifier Type<b></td><td><b>Capitalization Style<b></td><td><b>Example(s)<b></td></tr>
<tr><td>Abbreviations</td><td>Upper-ID</td><td>ID, REF</td></tr>
<tr><td>Namespaces</td><td>Pascal</td><td>AppDomain, System.IO</td></tr>
<tr><td>Classes &amp; Structs</td><td>Pascal</td><td>AppView</td></tr>
<tr><td>Constants & Enums</td><td>Pascal</td><td>TextStyles</td></tr>
<tr><td>Interfaces</td><td>Pascal</td><td>IEditableObject</td></tr>
<tr><td>Enum values</td><td>Pascal</td><td>TextStyles.BoldText</td></tr>
<tr><td>Property</td><td>Pascal</td><td>BackColor</td></tr>
<tr><td>Variables and Attributes</td><td>Pascal (public)<br/>Camel (private, protected, local)</td><td>WindowSize<br/>windowWidth, windowHeight</td></tr>
<tr><td>Methods</td><td>Pascal (public, private, protected)<br/>Camel (parameters)</td><td>ToString()<br/>SetFilter(string filterValue)</td></tr>
<tr><td>Events & Delegates</td><td>Pascal</td><td>MouseEventArgs</td></tr>
</table>

#### Namespaces

- The general rule for naming namespaces is to use the company name followed by the technology name and optionally the feature and design as follows. `CompanyName.TechnologyName[.Feature][.Design]`
- A nested namespace should have a dependency on types in the containing namespace. For example, the classes in the `System.Web.UI.Design` depend on the classes in `System.Web.UI`. However, the classes in `System.Web.UI` do not depend on the classes in `System.Web.UI.Design`.
- Use plural namespace names if it is semantically appropriate. Exceptions to this rule are brand names and abbreviations.
- Do not use the same name for a namespace and a class.

Examples:

```csharp
Esri.GameEngine.Providers;
Esri.GameEngine.Renderers;
Esri.GameEngine.Components;
```

#### Classes and structs

- Use a noun or noun phrase to name a class.
- Do not use the underscore character (_).
- Do not use a type prefix, such as C for class, on a class name.
- Where appropriate, use a compound word to name a derived class. The second part of the derived class's name should be the name of the base class. 

Examples:

```csharp
public class LightSceneNode : SceneNode
{
}

public class Vector3
{
}

public class TileMesh : Mesh
{
}
```

#### Interfaces

- Name interfaces with nouns or noun phrases, or adjectives that describe behavior.
- Prefix interface names with the letter I, to indicate that the type is an interface.
- Use similar names when you define a class/interface pair where the class is a standard implementation of the interface. The names should differ only by the letter I prefix on the interface name.
- Do not use the underscore character (_).

Examples:

```csharp
public interface ITileProvider
{
}

public interface ISceneComponentProvider
{
}

public class SceneComponentProvider : ISceneComponentProvider
{
}
```

#### Attributes

- Use a noun or noun phrase to name class or struct attributes.
- Do not use the underscore character (_).
- Use `readonly` when it's possible.

#### Variables

- Use a noun or noun phrase to name variables.

#### Enumerations

- Do not use an `Enum` suffix on *Enum* type names.
- Use a singular name for most *Enum* types, but use a plural name for *Enum* types that are bit fields.
- Always add the *FlagsAttribute* to a bit field *Enum* type.

Examples:

```csharp
public enum Color 
{ 
	Red, 
	Black, 
	White 
}

[Flags]
public enum PhoneService
{
	None = 0,
	LandLine = 1,
	Cell = 2,
	Fax = 4,
	Internet = 8,
	Other = 16
}
```

#### Static Fields

- Use nouns, noun phrases, or abbreviations of nouns to name static fields.
- Do not use a Hungarian notation prefix on static field names.
- It is recommended that you use static properties instead of public static fields whenever possible.

#### Parameters

- Use descriptive parameter names. Parameter names should be descriptive enough that the name of the parameter and its type can be used to determine its meaning in most scenarios.
- Use names that describe a parameter's meaning rather than names that describe a parameter's type. Development tools should provide meaningful information about a parameter's type. Therefore, a parameter's name can be put to better use by describing meaning.
- Do not use reserved parameters. Reserved parameters are private parameters that might be exposed in a future version if they are needed. Instead, if more data is needed in a future version of your class library, add a new overload for a method.

```csharp
public void Update(IReadOnlyDictionary<(uint, UInt64), SceneNodeCPUData> cpuSceneNodes, IReadOnlyDictionary<(uint, UInt64), SceneNodeGPUData> currentSceneNodes);
protected SceneComponentRenderer(MapType type, SceneComponentProvider.SceneComponentProvider sceneComponentProvider);
```

#### Methods

- Use verbs or verb phrases to name methods.
- Avoid unused parameters.

Examples:

```csharp
void UpdateSceneComponentsTransform();
void BlendImages(SortedList<int, ImageBlenderInput> input, RenderTexture output);
```

#### Properties

- Use a noun or noun phrase to name properties.
- Consider creating a property with the same name as its underlying type.

Examples:

```csharp
public enum Color 
{
	// Insert code for Enum here.
}

public class Control 
{
   	public Color Color 
	{ 
   		get 
      		{
      			// Insert code here.
		} 
      		set 
		{
			// Insert code here.
		} 
   	}
}
```

#### Events

- Use an `EventHandler` suffix on event handler names.
- Specify two parameters named `sender` and `e`. The `sender` parameter represents the object that raised the event. The `sender` parameter is always of type `object`, even if it is possible to use a more specific type. The state associated with the event is encapsulated in an instance of an event class named `e`. Use an appropriate and specific event class for the `e` parameter type.
- Name an event argument class with the `EventArgs` suffix.
- Consider naming events with a verb. For example, correctly named event names include `Clicked`, `Painting`, and `DroppedDown`.
- Use a gerund (the "ing" form of a verb) to create an event name that expresses the concept of pre-event, and a past-tense verb to represent post-event. For example, a Close event that can be canceled should have a `Closing` event and a `Closed` event. Do not use the BeforeXxx/AfterXxx naming pattern.
- Do not use a prefix or suffix on the event declaration on the type. For example, use `Close` instead of `OnClose`.
- In general, you should provide a protected method called OnXxx on types with events that can be overridden in a derived class. This method should only have the event parameter `e`, because the sender is always the instance of the type.

Examples:

```csharp
public delegate void MouseEventHandler(object sender, MouseEventArgs e)

public class MouseEventArgs : EventArgs 
{
	private int x;
	private int y;
	
	public MouseEventArgs(int x, int y) 
	{ 
		this.x = x; 
		this.y = y; 
	}
	  
	public int X 
	{ 
		get 
		{ 
			return x; 
		} 
	} 
	
	public int Y 
	{ 
		get 
		{ 
			return y; 
		} 
	} 
}
```
