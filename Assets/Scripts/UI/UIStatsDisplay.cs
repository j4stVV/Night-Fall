using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class UIStatsDisplay : MonoBehaviour
{
    public PlayerStats player;
    public bool displayCurrentHealth = false;
    public bool updateInEditor = false;
    TextMeshProUGUI statNames, statValues;

    private void OnEnable()
    {
        UpdateStatField();
    }

    private void OnDrawGizmosSelected()
    {
        if (updateInEditor) UpdateStatField();
    }

    public void UpdateStatField()
    {
        if (!player) return;

        if (!statNames) statNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!statValues) statValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        if (displayCurrentHealth)
        {
            names.AppendLine("Health");
            values.AppendLine(player.CurrentHealth.ToString());
        }

        FieldInfo[] fields = typeof(CharacterData.Stats).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            //Render stat names
            names.AppendLine(field.Name);

            //Get the stats values
            object val = field.GetValue(player.Stats);
            float fval = val is int ? (int)val : (float)val;

            PropertyAttribute attribute = (PropertyAttribute)PropertyAttribute.GetCustomAttribute(field, typeof(PropertyAttribute));
            if (attribute != null && field.FieldType == typeof(float))
            {
                float percentage = Mathf.Round(fval * 100 - 100);

                if (Mathf.Approximately(percentage, 0))
                {
                    values.Append('-').Append('\n');
                }
                else
                {
                    if (percentage > 0)
                    {
                        values.Append('+');
                    }
                    values.Append(percentage).Append('%').Append('\n');
                }
            }
            else
            {
                values.Append(fval).Append('\n');
            }

            //Updates the fields with the strings 
            statNames.text = PrettifyNames(names);
            statValues.text = PrettifyNames(values);
        }
    }

    public static string PrettifyNames(StringBuilder input)
    {
        if (input.Length <= 0) return string.Empty;

        StringBuilder result = new StringBuilder(); 
        char last = '\0';
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (last == '\0' || char.IsWhiteSpace(last))
            {
                c = char.ToUpper(c);
            }
            else if (char.IsUpper(c))
            {
                result.Append(' ');
            }
            result.Append(c);
            last = c;
        }

        return result.ToString();
    }
}
