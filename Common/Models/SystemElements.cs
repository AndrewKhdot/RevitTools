/*
 * Utworzone przez SharpDevelop.
 * Użytkownik: Khadatchuk
 * Data: 13.03.2026
 * Godzina: 12:17
 * 
 * Do zmiany tego szablonu użyj Narzędzia | Opcje | Kodowanie | Edycja Nagłówków Standardowych.
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace MyMacros
{
   public class SystemElements
    {
        public string SystemName { get; set; }
        public List<Element> Elements { get; set; }

        public SystemElements(string systemName)
        {
            SystemName = systemName;
            Elements = new List<Element>();
        }

        public void AddElement(Element element)
        {
            Elements.Add(element);
        }
        
public void RenumberElements(string paramName)
{
    // Список уже занятых номеров
    List<int> existingNumbers = new List<int>();

    // Список элементов без номера
    List<Element> unnumbered = new List<Element>();

    foreach (Element elem in Elements)
    {
        Parameter p = elem.LookupParameter(paramName);
        if (p != null && !string.IsNullOrEmpty(p.AsString()))
        {
            int num;
            if (int.TryParse(p.AsString(), out num))
            {
                existingNumbers.Add(num);
            }
            else
            {
                unnumbered.Add(elem);
            }
        }
        else
        {
            unnumbered.Add(elem);
        }
    }

    existingNumbers.Sort();
    int maxNumber = existingNumbers.Count > 0 ? existingNumbers.Max() : 0;

    // Находим недостающие номера
    List<int> missingNumbers = new List<int>();
    for (int i = 1; i <= maxNumber; i++)
    {
        if (!existingNumbers.Contains(i))
            missingNumbers.Add(i);
    }

    // Проставляем недостающие номера
    int index = 0;
    foreach (Element elem in unnumbered)
    {
        int newNumber;
        if (index < missingNumbers.Count)
        {
            newNumber = missingNumbers[index];
        }
        else
        {
            newNumber = ++maxNumber; // продолжаем нумерацию
        }

        Parameter p = elem.LookupParameter(paramName);
        if (p != null && !p.IsReadOnly)
        {
            p.Set(newNumber.ToString());
        }

        index++;
    }

	}
}
}
