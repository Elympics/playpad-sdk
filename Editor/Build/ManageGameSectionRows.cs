#nullable enable

using System;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.Build
{
    internal static class ManageGameSectionRows
    {
        public static void AddTextField(VisualElement container, string label, string tooltip, string value, Action<string> setValue)
        {
            var field = new TextField();
            field.SetValueWithoutNotify(value);
            _ = field.RegisterValueChangedCallback(evt => setValue(evt.newValue));
            AddRow(container, label, tooltip, field);
        }

        public static void AddRow(VisualElement container, string label, string tooltip, VisualElement field)
        {
            var row = new VisualElement();
            row.AddToClassList("elympics-field-row");
            var labelElement = new Label(label) { tooltip = tooltip };
            labelElement.AddToClassList("elympics-field-label");
            field.AddToClassList("elympics-endpoint-field");
            row.Add(labelElement);
            row.Add(field);
            container.Add(row);
        }
    }
}
