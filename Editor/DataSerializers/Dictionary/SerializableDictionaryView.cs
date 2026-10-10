using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExtInspectorTools.Editor
{
  /// <summary>
  /// UI Toolkit view of a <see cref="SerializableDictionary{TKey,TValue}"/>: key/value columns with a draggable
  /// splitter, reorderable rows, inline struct values and duplicate key highlighting.
  /// </summary>
  internal sealed class SerializableDictionaryView : VisualElement
  {
    private const string StyleSheetName = "ExtSerializableDictionary";
    private const float MinRatio = 0.15f;
    private const float MaxRatio = 0.85f;
    private static StyleSheet _styleSheet;

    private readonly SerializedProperty _property;
    private readonly SerializedProperty _pairs;
    private readonly SerializedProperty _ratio;
    private readonly List<int> _indices = new();
    private readonly HashSet<int> _duplicates = new();

    private readonly Foldout _foldout;
    private readonly Label _count;
    private readonly Label _duplicateBadge;
    private readonly VisualElement _columns;
    private readonly ListView _list;
    private readonly Label _empty;
    private int _arraySize = -1;
    private bool _dragging;

    public SerializableDictionaryView(SerializedProperty property)
    {
      _property = property.Copy();
      _pairs = property.FindPropertyRelative("pairs");
      _ratio = property.FindPropertyRelative("_editorKeyRatio");

      ApplyStyles();
      AddToClassList("ext-dict");

      _foldout = new Foldout { text = property.displayName, value = property.isExpanded };
      _foldout.AddToClassList("ext-dict__foldout");
      _foldout.RegisterValueChangedCallback(evt =>
      {
        if (evt.target != _foldout)
          return;
        _property.isExpanded = evt.newValue;
        _property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
      });

      Toggle header = _foldout.Q<Toggle>();
      header.AddToClassList("ext-dict__header");
      _count = Badge("ext-dict__count");
      _duplicateBadge = Badge("ext-dict__duplicates");
      var add = new Button(AddEntry) { text = "+", tooltip = "Add entry" };
      add.AddToClassList("ext-dict__add-small");
      add.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
      header.Add(_duplicateBadge);
      header.Add(_count);
      header.Add(add);

      _columns = CreateColumnHeader();
      _foldout.Add(_columns);

      _list = new ListView
      {
        itemsSource = _indices,
        reorderable = true,
        reorderMode = ListViewReorderMode.Animated,
        selectionType = SelectionType.None,
        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
        showAlternatingRowBackgrounds = AlternatingRowBackground.All,
        makeItem = MakeRow,
        bindItem = BindRow
      };
      _list.AddToClassList("ext-dict__list");
      _list.itemIndexChanged += (from, to) =>
      {
        _property.serializedObject.Update();
        _pairs.MoveArrayElement(from, to);
        _property.serializedObject.ApplyModifiedProperties();
        Rebuild();
      };
      _foldout.Add(_list);

      _empty = new Label("No entries. Press + to add one.");
      _empty.AddToClassList("ext-dict__empty");
      _foldout.Add(_empty);

      var footer = new Button(AddEntry) { text = "+ Add entry" };
      footer.AddToClassList("ext-dict__add");
      _foldout.Add(footer);

      Add(_foldout);

      this.TrackPropertyValue(_pairs, _ => OnPairsChanged());
      Rebuild();
    }

    private float Ratio => _ratio != null ? Mathf.Clamp(_ratio.floatValue <= 0f ? 0.4f : _ratio.floatValue, MinRatio, MaxRatio) : 0.4f;

    private static Label Badge(string className)
    {
      var badge = new Label();
      badge.AddToClassList("ext-dict__badge");
      badge.AddToClassList(className);
      return badge;
    }

    private void ApplyStyles()
    {
      if (_styleSheet == null)
      {
        string path = AssetDatabase.FindAssets($"{StyleSheetName} t:StyleSheet")
          .Select(AssetDatabase.GUIDToAssetPath)
          .FirstOrDefault(p => p.EndsWith($"/{StyleSheetName}.uss", StringComparison.Ordinal));
        if (path != null)
          _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
      }

      if (_styleSheet != null)
        styleSheets.Add(_styleSheet);
      EnableInClassList("ext-dict--light", !EditorGUIUtility.isProSkin);
    }

    #region Columns

    private VisualElement CreateColumnHeader()
    {
      var columns = new VisualElement();
      columns.AddToClassList("ext-dict__columns");

      var key = new Label("Key");
      key.AddToClassList("ext-dict__column");
      key.AddToClassList("ext-dict__key");

      var splitter = new VisualElement { tooltip = "Drag to resize the columns" };
      splitter.AddToClassList("ext-dict__splitter");
      splitter.Add(new VisualElement { pickingMode = PickingMode.Ignore });
      splitter.RegisterCallback<PointerDownEvent>(evt =>
      {
        _dragging = true;
        splitter.CapturePointer(evt.pointerId);
        evt.StopPropagation();
      });
      splitter.RegisterCallback<PointerMoveEvent>(evt =>
      {
        if (!_dragging)
          return;
        float local = columns.WorldToLocal(evt.position).x - ColumnsInset();
        float width = columns.contentRect.width - ColumnsInset() - RemoveColumnWidth();
        if (width > 0f)
          SetRatio(Mathf.Clamp(local / width, MinRatio, MaxRatio), save: false);
      });
      splitter.RegisterCallback<PointerUpEvent>(evt =>
      {
        if (!_dragging)
          return;
        _dragging = false;
        splitter.ReleasePointer(evt.pointerId);
        SetRatio(Ratio, save: true);
      });

      var value = new Label("Value");
      value.AddToClassList("ext-dict__column");
      value.AddToClassList("ext-dict__value");

      var spacer = new VisualElement();
      spacer.AddToClassList("ext-dict__remove-spacer");

      columns.Add(key);
      columns.Add(splitter);
      columns.Add(value);
      columns.Add(spacer);
      return columns;
    }

    // The list rows start after the reorder handle; the header is padded by the same amount in USS.
    private static float ColumnsInset() => 18f;
    private static float RemoveColumnWidth() => 26f;

    private void SetRatio(float ratio, bool save)
    {
      if (_ratio != null)
      {
        _ratio.floatValue = ratio;
        if (save)
          _property.serializedObject.ApplyModifiedProperties();
      }

      ApplyRatio(ratio);
    }

    private void ApplyRatio(float ratio)
    {
      var width = new StyleLength(new Length(ratio * 100f, LengthUnit.Percent));
      _columns.Q(className: "ext-dict__key").style.width = width;
      _list.Query(className: "ext-dict__row-key").ForEach(element => element.style.width = width);
    }

    #endregion

    #region Rows

    private VisualElement MakeRow()
    {
      var row = new VisualElement();
      row.AddToClassList("ext-dict__row");

      var key = new VisualElement();
      key.AddToClassList("ext-dict__row-key");
      var gap = new VisualElement();
      gap.AddToClassList("ext-dict__row-gap");
      var value = new VisualElement();
      value.AddToClassList("ext-dict__row-value");

      var remove = new Button { text = "✕", tooltip = "Remove entry" };
      remove.AddToClassList("ext-dict__remove");
      remove.clicked += () =>
      {
        if (row.userData is int index)
          RemoveEntry(index);
      };

      var duplicate = new Label("Duplicate");
      duplicate.AddToClassList("ext-dict__duplicate-tag");

      row.Add(key);
      row.Add(gap);
      row.Add(value);
      row.Add(remove);
      key.Add(duplicate);
      return row;
    }

    private void BindRow(VisualElement row, int index)
    {
      if (index >= _pairs.arraySize)
        return;

      row.userData = index;
      SerializedProperty pair = _pairs.GetArrayElementAtIndex(index);
      SerializedProperty key = pair.FindPropertyRelative("key");
      SerializedProperty value = pair.FindPropertyRelative("value");

      VisualElement keyContainer = row.Q(className: "ext-dict__row-key");
      VisualElement valueContainer = row.Q(className: "ext-dict__row-value");
      Label duplicateTag = row.Q<Label>(className: "ext-dict__duplicate-tag");

      keyContainer.Clear();
      valueContainer.Clear();
      keyContainer.style.width = new StyleLength(new Length(Ratio * 100f, LengthUnit.Percent));

      AddInline(keyContainer, key);
      keyContainer.Add(duplicateTag);
      AddInline(valueContainer, value);

      bool isDuplicate = _duplicates.Contains(index);
      row.EnableInClassList("ext-dict__row--duplicate", isDuplicate);
      duplicateTag.style.display = isDuplicate ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// Draws a key or value without its "Key"/"Value" label. Structs and classes are expanded inline;
    /// a struct with a single field shows just that field.
    /// </summary>
    private static void AddInline(VisualElement container, SerializedProperty property)
    {
      List<SerializedProperty> children = IsInlineable(property) ? Children(property).ToList() : null;
      if (children == null || children.Count == 0)
      {
        var field = new PropertyField(property, string.Empty);
        field.BindProperty(property);
        field.AddToClassList("ext-dict__field");
        container.Add(field);
        return;
      }

      if (children.Count == 1)
      {
        var single = new PropertyField(children[0], string.Empty) { tooltip = children[0].displayName };
        single.BindProperty(children[0]);
        single.AddToClassList("ext-dict__field");
        container.Add(single);
        return;
      }

      container.AddToClassList("ext-dict__row-value--struct");
      foreach (SerializedProperty child in children)
      {
        var field = new PropertyField(child);
        field.BindProperty(child);
        field.AddToClassList("ext-dict__field");
        field.AddToClassList("ext-dict__struct-field");
        container.Add(field);
      }
    }

    private static bool IsInlineable(SerializedProperty property) =>
      property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren && !property.isArray &&
      property.FindPropertyRelative("m_PersistentCalls") == null;

    private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
    {
      SerializedProperty iterator = property.Copy();
      SerializedProperty end = iterator.GetEndProperty();
      if (!iterator.NextVisible(true))
        yield break;

      do
      {
        if (SerializedProperty.EqualContents(iterator, end))
          yield break;
        yield return iterator.Copy();
      } while (iterator.NextVisible(false));
    }

    #endregion

    #region Data

    private void OnPairsChanged()
    {
      if (_pairs.arraySize != _arraySize)
      {
        Rebuild();
        return;
      }

      // Same rows: only refresh the duplicate markers, so fields keep their focus while typing.
      FindDuplicates();
      _list.Query(className: "ext-dict__row").ForEach(row =>
      {
        if (row.userData is not int index)
          return;
        bool isDuplicate = _duplicates.Contains(index);
        row.EnableInClassList("ext-dict__row--duplicate", isDuplicate);
        Label tag = row.Q<Label>(className: "ext-dict__duplicate-tag");
        if (tag != null)
          tag.style.display = isDuplicate ? DisplayStyle.Flex : DisplayStyle.None;
      });
      UpdateHeader();
    }

    private void Rebuild()
    {
      _property.serializedObject.Update();
      _arraySize = _pairs.arraySize;
      _indices.Clear();
      for (int i = 0; i < _arraySize; i++)
        _indices.Add(i);

      FindDuplicates();
      _list.Rebuild();
      ApplyRatio(Ratio);
      UpdateHeader();
    }

    private void FindDuplicates()
    {
      _duplicates.Clear();
      for (int i = 0; i < _pairs.arraySize; i++)
      {
        SerializedProperty key = _pairs.GetArrayElementAtIndex(i).FindPropertyRelative("key");
        for (int j = 0; j < i; j++)
        {
          if (!SerializedProperty.DataEquals(key, _pairs.GetArrayElementAtIndex(j).FindPropertyRelative("key")))
            continue;
          _duplicates.Add(i);
          break;
        }
      }
    }

    private void UpdateHeader()
    {
      int count = _pairs.arraySize;
      _count.text = count == 1 ? "1 entry" : $"{count} entries";
      _duplicateBadge.text = _duplicates.Count == 1 ? "1 duplicate key" : $"{_duplicates.Count} duplicate keys";
      _duplicateBadge.style.display = _duplicates.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
      _duplicateBadge.tooltip = "Entries with a repeated key are ignored at runtime.";
      bool empty = count == 0;
      _empty.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
      _columns.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
      _list.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void AddEntry()
    {
      _property.serializedObject.Update();
      _pairs.arraySize++;
      _property.serializedObject.ApplyModifiedProperties();
      _property.isExpanded = true;
      _foldout.value = true;
      Rebuild();
    }

    private void RemoveEntry(int index)
    {
      _property.serializedObject.Update();
      if (index < 0 || index >= _pairs.arraySize)
        return;
      _pairs.DeleteArrayElementAtIndex(index);
      _property.serializedObject.ApplyModifiedProperties();
      Rebuild();
    }

    #endregion
  }
}
