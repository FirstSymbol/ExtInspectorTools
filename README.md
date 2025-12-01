
# ExtInspectorTools
## Serializers
### SerializableType<TParent> class
Allows you to select the type that inherits from the specified class. In the example, the classes Armor, Health, Mana, Speed, and Endurance are inherited from the StatBase class.<br>
```Csharp
public SerializableType<StatBase> payloadType;
```
Choosing type:<br><br>
![Choose type](https://github.com/user-attachments/assets/28bc49ac-27ee-4fa8-89c8-72e5d8f0d7f3)<br>
<br>How it looks at inspector:<br><br>
![Looks at inspector](https://github.com/user-attachments/assets/50fbd02a-d5d9-414b-b2ca-e10fc0e122e0)<br>

### SerializableDictionary class
Allows you serialize a dictionary in inspector.<br>
```Csharp
public SerializableDictionary<int, string>  wordDictionary = new();
```
<br>How it looks at inspector:<br><br>
![Looks at inspector](https://github.com/user-attachments/assets/211b60fa-ef34-4c89-970a-ed2ed60a43f7)<br>

## Attributes
### RequireNotNull Attribute
This attribute prevents the game from starting and sends an error to the console if the field with this attribute is Null at the launch stage.<br>
```Csharp
    [RequireNotNull] public InventoryViewLinker InventoryViewLinker;
```
Console log:<br><br>
![](https://github.com/user-attachments/assets/2280dde7-47cd-4ea3-8340-34ff7ada0f25)
