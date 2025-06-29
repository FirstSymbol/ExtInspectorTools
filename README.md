# ExtInspectorTools
## TypeReference<TParent> class
Allows you to select the type that inherits from the specified class. In the example, the classes Armor, Health, Mana, Speed, and Endurance are inherited from the StatBase class.<br>
```Csharp
    public TypeReference<StatBase> payloadType;
```
Choosing type:<br><br>
![Choose type](https://github.com/user-attachments/assets/28bc49ac-27ee-4fa8-89c8-72e5d8f0d7f3)<br>
<br>How it looks at inspector:<br><br>
![Looks at inspector](https://github.com/user-attachments/assets/50fbd02a-d5d9-414b-b2ca-e10fc0e122e0)<br>

## RequireNotNull Attribute
This attribute prevents the game from starting and sends an error to the console if the field with this attribute is Null at the launch stage.<br>
```Csharp
    [RequireNotNull] public InventoryViewLinker InventoryViewLinker;
```
Console log:<br><br>
![](https://github.com/user-attachments/assets/2280dde7-47cd-4ea3-8340-34ff7ada0f25)

## SerializableDictionary<TKey, TValue>
Just a serializable dictionary in the inspector.
