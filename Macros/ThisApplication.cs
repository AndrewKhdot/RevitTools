/*
 * Created by SharpDevelop.
 * User: Khadatchuk
 * Date: 24.04.2025
 * Time: 12:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using System.Text;
using System.Reflection;

namespace MyMacros
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.DB.Macros.AddInId("B745803F-AD2A-4D83-871C-476407CB9CA9")]
    
	public partial class ThisApplication
	{
		private void Module_Startup(object sender, EventArgs e)
		{

		}

		private void Module_Shutdown(object sender, EventArgs e)
		{

		}

		#region Revit Macros generated code
		private void InternalStartup()
		{
			this.Startup += new System.EventHandler(Module_Startup);
			this.Shutdown += new System.EventHandler(Module_Shutdown);
		}
		#endregion
		public void FindElement()
		{
			UIDocument uidoc = this.ActiveUIDocument;
		    Document doc = uidoc.Document;
		    
		
		    string input = ShowNameInputDialog();
		
		    if (string.IsNullOrWhiteSpace(input))
		    {
		        TaskDialog.Show("Результат", "Вы не ввели ID.");
		        return;
		    }
			int elementIdValue;
		    if (!int.TryParse(input, out elementIdValue))
		    {
		        TaskDialog.Show("Ошибка", "Введите корректный числовой ID.");
		        return;
		    }
		
		    Element element = doc.GetElement(new ElementId(elementIdValue));
		    if (element == null)
		    {
		    	string message =  string.Format("Элемент с ID {0} не найден.", elementIdValue);
		        TaskDialog.Show("Ошибка", message);
		        return;
		    }
		
		    XYZ point = null;
		    Location loc = element.Location;
			
			LocationPoint locPoint = loc as LocationPoint;
			if (locPoint != null)
			{
			    point = locPoint.Point;
			}
			else
			{
			    LocationCurve locCurve = loc as LocationCurve;
			    if (locCurve != null)
			    {
			        point = locCurve.Curve.Evaluate(0.5, true);
			    }
			    else
			    {
			        TaskDialog.Show("Ошибка", "Не удалось определить точку расположения элемента.");
			        return;
			    }
			}
		
			    var spatialElements = new FilteredElementCollector(doc)
        .OfClass(typeof(SpatialElement))
        .Cast<SpatialElement>();

    // Создаем форму для вывода результатов
	System.Windows.Forms.Form tableForm = new System.Windows.Forms.Form();
    tableForm.Text = "Результаты поиска";
    tableForm.Width = 600;
    tableForm.Height = 400;
    tableForm.StartPosition = FormStartPosition.CenterScreen;

    DataGridView grid = new DataGridView();
    grid.Dock = DockStyle.Fill;
    grid.ColumnCount = 3;
    grid.Columns[0].Name = "Номер";
    grid.Columns[1].Name = "Имя";
    grid.Columns[2].Name = "Тип";

    tableForm.Controls.Add(grid);

    bool found = false;

    foreach (var se in spatialElements)
    {
        Room room = se as Room;
        if (room != null)
        {
            if (room.IsPointInRoom(point))
            {
                grid.Rows.Add(room.Number, room.Name, "Помещение");
                found = true;
            }
        }

        Autodesk.Revit.DB.Mechanical.Space space = se as Autodesk.Revit.DB.Mechanical.Space;
        if (space != null)
        {
            if (space.IsPointInSpace(point))
            {
                grid.Rows.Add(space.Number, space.Name, "Инженерное пространство");
                found = true;
            }
        }
        // Можно добавить еще обработку "Zone", если потребуется
    }

    if (found)
    {
        tableForm.ShowDialog();
    }
    else
    {
        TaskDialog.Show("Результат", "Элемент не найден ни в одном пространстве.");
    }

		
		    TaskDialog.Show("Результат", "Элемент находится вне инженерных пространств.");
		}
		
				public void ChangeParamSilensers()
		{
			    UIDocument uidoc = this.ActiveUIDocument;
    			Document doc = uidoc.Document;

			    // Коллектор инженерных аксессуаров вентиляции
			    var accessories = new FilteredElementCollector(doc)
			        .OfCategory(BuiltInCategory.OST_DuctAccessory) // если это не то, поменяем категорию
			        .WhereElementIsNotElementType()
			        .ToElements();
			    
			    
			    int totalCount = accessories.Count;
			    int withModelCount = 0;
			    int silenserNumber = 0;
			    int checkNum = 0;
			    int[] freeNumb = new int[1];
			    
			    int[] fullAr;
			    int maxNum = 0;
			    List<int> numbersInt = new List<int>();
			    
			    
				
			    
			    foreach (var accessory in accessories)
    			 {
    		        ElementId typeId = accessory.GetTypeId();
			        Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;

							
			        // Ищем параметр "Model" в типе
			        Parameter modelParam = typeElem.LookupParameter("Model");
					Parameter targetFourParam = accessory.LookupParameter("MC Object Variable 4");
					
					if (modelParam != null && modelParam.HasValue)
			        {
						
			            string modelValue = modelParam.AsString();

			            if (!string.IsNullOrEmpty(modelValue))
			            {
			            
			            	if (modelValue.StartsWith("MS") || modelValue.StartsWith("CA") || modelValue.StartsWith("CS"))
			            	{
			            		if (targetFourParam != null && !targetFourParam.IsReadOnly )
			            	    {
				               	 	string numSt = targetFourParam.AsString() ?? "";
				               	 	if(numSt.Length>=1)
				               	 	{
				               	 		checkNum++;
				               	 		int curNum = Convert.ToInt32(numSt);
				               	 		if(curNum > maxNum)
				               	 		{
				               	 			maxNum = curNum;
				               	 		}
				               	 		numbersInt.Add(curNum);
				               	 	}
			            			
				            	}
			            	}
			            }
			         }
					
    			 }
    			 
			     numbersInt.Sort();
			     int[] numDigAr = numbersInt.ToArray();

	    	     if(numDigAr.Length != 0)
		    			 {
		    			 	fullAr = new int[maxNum];
		    			 	for (int i = 0; i < fullAr.Length; i++) 
		    			 	{
		    			 		fullAr[i] = i+1;
		    			 	}
		    			 	freeNumb = fullAr.Except(numDigAr).ToArray();
    			 			silenserNumber = maxNum;
		   			 	 }
	  	
	   			 
    			 
    			 
    			 //silenserNumber = 0;
    			 
		    
			        Transaction t = new Transaction(doc, "Копирование параметров");
    				t.Start();
    				


						    
			    foreach (var accessory in accessories)
				{
				    // Получаем параметр "Model"
				    ElementId typeId = accessory.GetTypeId();
			        Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;
			
			        // Ищем параметр "Model" в типе
			        Parameter modelParam = typeElem.LookupParameter("Model");
					Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
					Parameter targetTwoParam = accessory.LookupParameter("MC Object Variable 2");
					Parameter targetThirdParam = accessory.LookupParameter("MC Object Variable 3");
					Parameter targetFourParam = accessory.LookupParameter("MC Object Variable 4");
					Parameter firstSilenserType = accessory.LookupParameter("TX_Δpst");
					Parameter secondSilenserType = accessory.LookupParameter("TX_Static_differential_pressure");
					Parameter phaseCreatedParam = accessory.get_Parameter(BuiltInParameter.PHASE_CREATED);
					
//					if (CheckPhase(phaseCreatedParam, doc))
//						continue;
				    if (modelParam != null && modelParam.HasValue)
			        {
			            string modelValue = modelParam.AsString();

			            if (!string.IsNullOrEmpty(modelValue))
			            {
			            
			            	if (modelValue.StartsWith("MS"))
				            {
				            	string[] size = modelValue.Split(new char[] { '/' });
				            	string[] sizeParam = size[1].Split(new char[] { 'x' });
				            	
				            	if (targetParam != null && !targetParam.IsReadOnly)
				            	{
				            		targetParam.Set(sizeParam[0] + "x" + sizeParam[1]);
				            	}
												            	
				               	if (targetTwoParam != null && !targetParam.IsReadOnly)
				            	{
				            		targetTwoParam.Set(sizeParam[2]);
				            	}
				               	if (targetFourParam != null && !targetParam.IsReadOnly)
				            	{
				               		
				               		string val = targetFourParam.AsString();
    								if (string.IsNullOrEmpty(val))
				               		{
				               	 		if (freeNumb.Length > 0)
						                {
				               	 			targetFourParam.Set(freeNumb[0].ToString());
						                    int[] ar = new int[freeNumb.Length - 1];
						                    Array.Copy(freeNumb, 1, ar, 0, freeNumb.Length - 1);
						                    freeNumb = ar;
						                }
						                else 
						                {
						                    silenserNumber = silenserNumber + 1;
				            				targetFourParam.Set(silenserNumber.ToString());
						                }
				               		}
				            	}
				               		if(firstSilenserType!= null && modelParam.HasValue && targetThirdParam != null && !targetThirdParam.IsReadOnly)
			            		{
				               			
			            			targetThirdParam.Set(firstSilenserType.AsValueString());
			            		}
				               		if(secondSilenserType!= null && modelParam.HasValue && targetThirdParam != null && !targetThirdParam.IsReadOnly)
			            		{
				               			
			            			targetThirdParam.Set(secondSilenserType.AsValueString());
			            		}
				               		
				               	
				            }
			            	if (modelValue.StartsWith("CA") || modelValue.StartsWith("CS"))
				            {
				            	string[] size = modelValue.Split(new char[] { '/' });
				            	string[] sizeParam = size[1].Split(new char[] { 'x' });
				            	
				            	if (targetParam != null && !targetParam.IsReadOnly)
				            	{
				            		targetParam.Set("⌀" + sizeParam[0]);
				            	}
												            	
				               	if (targetTwoParam != null && !targetParam.IsReadOnly)
				            	{
				            		targetTwoParam.Set(sizeParam[1]);
				            	}
				               	if (targetFourParam != null && !targetParam.IsReadOnly)
				            	{
				               		
				               		string val = targetFourParam.AsString();
    								if (string.IsNullOrEmpty(val))
				               		{
				               	 		if (freeNumb.Length > 0)
						                {
				               	 			targetFourParam.Set(freeNumb[0].ToString());
						                    int[] ar = new int[freeNumb.Length - 1];
						                    Array.Copy(freeNumb, 1, ar, 0, freeNumb.Length - 1);
						                    freeNumb = ar;
						                }
						                else 
						                {
						                    silenserNumber = silenserNumber + 1;
				            				targetFourParam.Set(silenserNumber.ToString());
						                }
				               		}
				            	}
				               	if(firstSilenserType!= null && modelParam.HasValue && targetThirdParam != null && !targetThirdParam.IsReadOnly)
			            		{
				               			
			            			targetThirdParam.Set(firstSilenserType.AsValueString());
			            		}
				               		if(secondSilenserType!= null && modelParam.HasValue && targetThirdParam != null && !targetThirdParam.IsReadOnly)
			            		{
				               			
			            			targetThirdParam.Set(secondSilenserType.AsValueString());
			            		}
				            }
			               
			            }
			        }
							    
				}
			     t.Commit();
			    
			   TaskDialog.Show("Результаты подсчета",
			                     "Всего элементов: " + checkNum.ToString() +
		        "\nС заполненным параметром 'Model': " + withModelCount.ToString());
    			 
				}
		
		private string ShowNameInputDialog()
		{
		    System.Windows.Forms.Form inputForm = new System.Windows.Forms.Form();
		    inputForm.Text = "Введите ваше имя";
		    inputForm.Width = 300;
		    inputForm.Height = 150;
		    inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
		    inputForm.StartPosition = FormStartPosition.CenterScreen;
		    inputForm.MaximizeBox = false;
		    inputForm.MinimizeBox = false;
		
		    Label label = new Label() { Left = 20, Top = 20, Text = "Ваше имя:", AutoSize = true };
		    System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 20, Top = 45, Width = 240 };
		    Button confirmButton = new Button() { Text = "ОК", Left = 100, Width = 80, Top = 80, DialogResult = DialogResult.OK };
		
		    inputForm.Controls.Add(label);
		    inputForm.Controls.Add(textBox);
		    inputForm.Controls.Add(confirmButton);
		    inputForm.AcceptButton = confirmButton;
		
		    return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
		}
		
public void KlapyPPNameNulling()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    // Коллектор инженерных аксессуаров вентиляции
    var accessories = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_DuctAccessory)
        .WhereElementIsNotElementType()
        .ToElements();

    using (Transaction trans = new Transaction(doc, "Очистка MC Object Variable 1"))
    {
        trans.Start();

        foreach (var accessory in accessories)
        {
            ElementId typeId = accessory.GetTypeId();
            Element typeElem = doc.GetElement(typeId);
            if (typeElem == null)
                continue;

            Parameter modelParam = typeElem.LookupParameter("MC Product Code");
            if (modelParam == null)
                continue;

            string modelValue = modelParam.AsString();
            if (string.IsNullOrEmpty(modelValue))
                continue;

            bool isCircleFD = modelValue.StartsWith("DKIR1");
            bool isRecFD = modelValue.StartsWith("FDS");

            if (!isCircleFD && !isRecFD)
                continue;

            Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
            if (targetParam == null || targetParam.IsReadOnly)
                continue;

            targetParam.Set("");
        }

        trans.Commit();
    }
}

		
		public void KlapyPP()
		{
			    UIDocument uidoc = this.ActiveUIDocument;
    			Document doc = uidoc.Document;

			    // Коллектор инженерных аксессуаров вентиляции
			    var accessories = new FilteredElementCollector(doc)
			        .OfCategory(BuiltInCategory.OST_DuctAccessory) // если это не то, поменяем категорию
			        .WhereElementIsNotElementType()
			        .ToElements();

			    // Коллектор всех пространств
				var spaces = new FilteredElementCollector(doc)
				    .OfClass(typeof(SpatialElement))
				    .Cast<SpatialElement>()
				    .Where(x => x is Space) // оставляем только Space
				    .Cast<Space>() // кастуем в Space
				    .ToList();

    int updatedCount = 0;
    int maxNum = 0;
	int[] freeNumb = new int[1];
	List<int> numbersInt = new List<int>();

    using (Transaction trans = new Transaction(doc, "Заполнить MC Object Variable 3"))
    {
        trans.Start();
        foreach (var accessory in accessories)
        {
        	ElementId typeId = accessory.GetTypeId();
			Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;
            // Читаем параметр "MC Object Variable 1" MC Object Variable 1
            Parameter modelParam = typeElem.LookupParameter("MC Product Code");
            Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
            if (modelParam == null || string.IsNullOrWhiteSpace(modelParam.AsString()))
                continue;
            bool isCircleFD = modelParam.AsString().StartsWith("DKIR1");
            bool isRecFD = modelParam.AsString().StartsWith("FDS");
            if (!isCircleFD && !isRecFD)
            	continue;
            string numKP = targetParam.AsString();
		    	if(numKP.Length>=1)
					{
		    			int numberKP = Convert.ToInt32(numKP);
				    	numbersInt.Add(numberKP);
				    	if(numberKP > maxNum)
				    	{
				    		maxNum = numberKP;
				    	}
				    }
            
        }
        
        numbersInt.Sort();
	    int[] numDigAr = numbersInt.ToArray();

	    	     if(numDigAr.Length != 0)
		    			 {
	    	     	int[] fullAr = new int[maxNum];
		    			 	for (int i = 0; i < fullAr.Length; i++) 
		    			 	{
		    			 		fullAr[i] = i+1;
		    			 	}
		    			 	freeNumb = fullAr.Except(numDigAr).ToArray();
		   			 	 }
        
        updatedCount = maxNum;
        string m = "";
        
        string f = "";
        for (int i = 0; i < numDigAr.Length; i++) 
		    			 	{
		    			 		f = f + numDigAr[i].ToString();
		    			 	}
        
        for (int i = 0; i < freeNumb.Length; i++) 
		    			 	{
		    			 		m = m + freeNumb[i].ToString();
		    			 	}
        
        string finalMessage1 = string.Format("Обновлено элементов: {0}", m);
        TaskDialog.Show("Результат", finalMessage1);
        finalMessage1 = string.Format("Обновлено элементов: {0}", f);
        TaskDialog.Show("Результат", finalMessage1);


        foreach (var accessory in accessories)
        {
        	// Получаем параметр "Model"
				    ElementId typeId = accessory.GetTypeId();
			        Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;
            // Читаем параметр "MC Object Variable 1" MC Object Variable 1
            Parameter modelParam = typeElem.LookupParameter("MC Product Code");
            Parameter size1 = typeElem.LookupParameter("MC Connection Size 1");
            Parameter size2 = typeElem.LookupParameter("MC Connection Size 2");
            Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
            Parameter phaseCreatedParam = accessory.get_Parameter(BuiltInParameter.PHASE_CREATED);


         
            if (modelParam == null || string.IsNullOrWhiteSpace(modelParam.AsString()))
                continue; // Если параметр не существует или пустой — пропускаем

            if(CheckPhase(phaseCreatedParam, doc))
            	continue;

//		    if (phaseCreatedParam == null)
//          		continue;
//		   ElementId phaseId = phaseCreatedParam.AsElementId();
//           Element phaseElem = doc.GetElement(phaseId);
//           if (phaseElem == null)
//            	continue;
//           string phaseName = phaseElem.Name;
//           if(phaseName != "Nowa konstrukcja")
//            	continue;
//		   if(updatedCount == 3)
//		            {
//		            	TaskDialog.Show("Фаза создания", "Фаза: ");
//		            }
		            


//			            
           bool isCircleFD = modelParam.AsString().StartsWith("DKIR1");
           bool isRecFD = modelParam.AsString().StartsWith("FDS");
            if (!isCircleFD && !isRecFD)
            	continue;
            if (targetParam != null && !targetParam.IsReadOnly)
				{
				               		
					string val = targetParam.AsString();
    				if (string.IsNullOrEmpty(val))
				    	{
				        	if (freeNumb.Length > 0)
						    	{
				               		targetParam.Set(freeNumb[0].ToString());				               		 
						            int[] ar = new int[freeNumb.Length - 1];
						            Array.Copy(freeNumb, 1, ar, 0, freeNumb.Length - 1);
						            freeNumb = ar;
						        }
						    else 
						        {
						    		       
						            ++updatedCount;
                                    targetParam.Set(updatedCount.ToString());
                                    
						        }
				          }
            	 }
				             
            

//            if(updatedCount == 30)
//            {
//            	if (phaseCreatedParam != null)
//{
//    ElementId phaseId = phaseCreatedParam.AsElementId();
//    Element phaseElem = doc.GetElement(phaseId);
//
//    if (phaseElem != null)
//    {
//        string phaseName = phaseElem.Name;
//        TaskDialog.Show("Фаза создания", "Фаза: " + phaseName);
//    }
//}
//else
//{
//    TaskDialog.Show("Ошибка", "Параметр 'Фаза создания' не найден.");
//}
//            }
            // Получаем точку элемента
            XYZ point = null;
            Location location = accessory.Location;
			LocationPoint locPoint = location as LocationPoint;
			if (locPoint != null)
			{
			    point = locPoint.Point;
			}
			LocationCurve locCurve = location as LocationCurve;
			if (locCurve != null)
			{
			    point = locCurve.Curve.Evaluate(0.5, true); // середина кривой
			}

            if (point == null)
                continue; // нет геометрии — пропускаем

            // Ищем Space для этой точки
            foreach (var space in spaces)
            {
                if (space.IsPointInSpace(point))
                {
                    // Формируем строку "Номер - Название"
                    Parameter name = space.LookupParameter("Nazwa");
                    
                    string newValue = string.Format("({0}) {1}", space.Number, name.AsString());
                    string newValue2 ="";
                    if(isCircleFD)
                    {
                    	
                    	newValue2 = string.Format("jednopłaszczyznowa przeciwpożarowa klapa okrągła d={0}", size1.AsValueString());
                    }
                    
                       if(isRecFD)
                    {
                       	newValue2 = string.Format("jednopłaszczyznowa przeciwpożarowa klapa prostokątna {0}x{1}", size1.AsValueString().TrimEnd(new char[] { 'm'}).TrimEnd(new char[] { 'm'}).Trim(), size2.AsValueString());
                    }
	
                    // Записываем в "MC Object Variable 3"
                    Parameter mcVar3 = accessory.LookupParameter("MC Object Variable 3");
                    Parameter mcVar2 = accessory.LookupParameter("MC Object Variable 2");
                    
                    if (mcVar3 != null && !mcVar3.IsReadOnly)
                    {
                        mcVar3.Set(newValue);
                        mcVar2.Set(newValue2);
                    }

                    break; // нашли пространство — больше искать не надо
                }
            }
        }

        trans.Commit();
    }
    string finalMessage = string.Format("Обновлено элементов: {0}", updatedCount);
    TaskDialog.Show("Результат", finalMessage);
		}
		
		public void Regul()
		{
			    UIDocument uidoc = this.ActiveUIDocument;
    			Document doc = uidoc.Document;

			    // Коллектор инженерных аксессуаров вентиляции
			    var accessories = new FilteredElementCollector(doc)
			        .OfCategory(BuiltInCategory.OST_DuctAccessory) // если это не то, поменяем категорию
			        .WhereElementIsNotElementType()
			        .ToElements();

			    // Коллектор всех пространств
				var spaces = new FilteredElementCollector(doc)
				    .OfClass(typeof(SpatialElement))
				    .Cast<SpatialElement>()
				    .Where(x => x is Space) // оставляем только Space
				    .Cast<Space>() // кастуем в Space
				    .ToList();

    int updatedCount = 0;
    int maxNum = 0;
	int[] freeNumb = new int[1];
	List<int> numbersInt = new List<int>();

    using (Transaction trans = new Transaction(doc, "Заполнить MC Object Variable 3"))
    {
        trans.Start();
        foreach (var accessory in accessories)
        {
        	ElementId typeId = accessory.GetTypeId();
			Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;
            // Читаем параметр "MC Object Variable 1" MC Object Variable 1
            Parameter modelParam = typeElem.LookupParameter("MC Product Code");
            Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
            if (modelParam == null || string.IsNullOrWhiteSpace(modelParam.AsString()))
                continue;
            bool isCircleFD = modelParam.AsString().StartsWith("DKIR1");
            bool isRecFD = modelParam.AsString().StartsWith("FDS");
            if (!isCircleFD && !isRecFD)
            	continue;
            string numKP = targetParam.AsString();
		    	if(numKP.Length>1)
					{
		    			int numberKP = Convert.ToInt32(numKP);
				    	numbersInt.Add(numberKP);
				    	if(numberKP > maxNum)
				    	{
				    		maxNum = numberKP;
				    	}
				    }
            
        }
        
        numbersInt.Sort();
	    int[] numDigAr = numbersInt.ToArray();

	    	     if(numDigAr.Length != 0)
		    			 {
	    	     	int[] fullAr = new int[maxNum];
		    			 	for (int i = 0; i < fullAr.Length; i++) 
		    			 	{
		    			 		fullAr[i] = i+1;
		    			 	}
		    			 	freeNumb = fullAr.Except(numDigAr).ToArray();
		   			 	 }
        
        
        

        foreach (var accessory in accessories)
        {
        	// Получаем параметр "Model"
				    ElementId typeId = accessory.GetTypeId();
			        Element typeElem = doc.GetElement(typeId);
			
			        if (typeElem == null)
			            continue;
            // Читаем параметр "MC Object Variable 1" MC Object Variable 1
            Parameter modelParam = typeElem.LookupParameter("MC Product Code");
            Parameter size1 = typeElem.LookupParameter("MC Connection Size 1");
            Parameter size2 = typeElem.LookupParameter("MC Connection Size 2");
            Parameter targetParam = accessory.LookupParameter("MC Object Variable 1");
            Parameter phaseCreatedParam = accessory.get_Parameter(BuiltInParameter.PHASE_CREATED);


         
            if (modelParam == null || string.IsNullOrWhiteSpace(modelParam.AsString()))
                continue; // Если параметр не существует или пустой — пропускаем

            if(CheckPhase(phaseCreatedParam, doc))
            	continue;

//		    if (phaseCreatedParam == null)
//          		continue;
//		   ElementId phaseId = phaseCreatedParam.AsElementId();
//           Element phaseElem = doc.GetElement(phaseId);
//           if (phaseElem == null)
//            	continue;
//           string phaseName = phaseElem.Name;
//           if(phaseName != "Nowa konstrukcja")
//            	continue;
//		   if(updatedCount == 3)
//		            {
//		            	TaskDialog.Show("Фаза создания", "Фаза: ");
//		            }
		            


//			            
           bool isCircleFD = modelParam.AsString().StartsWith("DKIR1");
           bool isRecFD = modelParam.AsString().StartsWith("FDS");
            if (!isCircleFD && !isRecFD)
            	continue;
            if (targetParam != null && !targetParam.IsReadOnly)
				{
				               		
					string val = targetParam.AsString();
    				if (string.IsNullOrEmpty(val))
				    	{
				        	if (freeNumb.Length > 0)
						    	{
				               		targetParam.Set(freeNumb[0].ToString());
						            int[] ar = new int[freeNumb.Length - 1];
						            Array.Copy(freeNumb, 1, ar, 0, freeNumb.Length - 1);
						            freeNumb = ar;
						        }
						    else 
						        {
						            ++updatedCount;
                                    targetParam.Set(updatedCount.ToString());
						        }
				          }
            	 }
				             
            

//            if(updatedCount == 30)
//            {
//            	if (phaseCreatedParam != null)
//{
//    ElementId phaseId = phaseCreatedParam.AsElementId();
//    Element phaseElem = doc.GetElement(phaseId);
//
//    if (phaseElem != null)
//    {
//        string phaseName = phaseElem.Name;
//        TaskDialog.Show("Фаза создания", "Фаза: " + phaseName);
//    }
//}
//else
//{
//    TaskDialog.Show("Ошибка", "Параметр 'Фаза создания' не найден.");
//}
//            }
            // Получаем точку элемента
            XYZ point = null;
            Location location = accessory.Location;
			LocationPoint locPoint = location as LocationPoint;
			if (locPoint != null)
			{
			    point = locPoint.Point;
			}
			LocationCurve locCurve = location as LocationCurve;
			if (locCurve != null)
			{
			    point = locCurve.Curve.Evaluate(0.5, true); // середина кривой
			}

            if (point == null)
                continue; // нет геометрии — пропускаем

            // Ищем Space для этой точки
            foreach (var space in spaces)
            {
                if (space.IsPointInSpace(point))
                {
                    // Формируем строку "Номер - Название"
                    Parameter name = space.LookupParameter("Nazwa");
                    
                    string newValue = string.Format("({0}) {1}", space.Number, name.AsString());
                    string newValue2 ="";
                    if(isCircleFD)
                    {
                    	
                    	newValue2 = string.Format("jednopłaszczyznowa przeciwpożarowa klapa okrągła d={0}", size1.AsValueString());
                    }
                    
                       if(isRecFD)
                    {
                       	newValue2 = string.Format("jednopłaszczyznowa przeciwpożarowa klapa prostokątna {0}x{1}", size1.AsValueString().TrimEnd(new char[] { 'm'}).TrimEnd(new char[] { 'm'}).Trim(), size2.AsValueString());
                    }
	
                    // Записываем в "MC Object Variable 3"
                    Parameter mcVar3 = accessory.LookupParameter("MC Object Variable 3");
                    Parameter mcVar2 = accessory.LookupParameter("MC Object Variable 2");
                    
                    if (mcVar3 != null && !mcVar3.IsReadOnly)
                    {
                        mcVar3.Set(newValue);
                        mcVar2.Set(newValue2);
                    }

                    break; // нашли пространство — больше искать не надо
                }
            }
        }

        trans.Commit();
    }
    string finalMessage = string.Format("Обновлено элементов: {0}", updatedCount);
    TaskDialog.Show("Результат", finalMessage);
		}
		
		public bool CheckPhase(Parameter phaseCreatedParam, Document doc)
		{
			if (phaseCreatedParam == null)
          		return false;
		   ElementId phaseId = phaseCreatedParam.AsElementId();
           Element phaseElem = doc.GetElement(phaseId);
           if (phaseElem == null)
            	return false;
           string phaseName = phaseElem.Name;
           if(phaseName != "Nowa konstrukcja")
            	return true;
           else
           	 return false;
		}
		
		public void CopyOffsetParamToSpaceComment()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    // Найдём связанный архитектурный файл 
    var linkedInstance = new FilteredElementCollector(doc)
        .OfClass(typeof(RevitLinkInstance))
        .Cast<RevitLinkInstance>()
        .FirstOrDefault(inst => inst.Name.ToLower().Contains("-ar-")); // Измени при необходимости

    if (linkedInstance == null)
    {
        TaskDialog.Show("Ошибка", "Связанный файл архитектуры не найден.");
        return;
    }

    Document linkedDoc = linkedInstance.GetLinkDocument();

    if (linkedDoc == null)
    {
        TaskDialog.Show("Ошибка", "Связанный файл не загружен. Проверь подключение.");
        return;
    }

    // Соберём помещения из архитектурной модели
    var rooms = new FilteredElementCollector(linkedDoc)
        .OfCategory(BuiltInCategory.OST_Rooms)
        .WhereElementIsNotElementType()
        .Cast<Room>()
        .ToList();

    // Соберём пространства из текущей модели
    var spaces = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_MEPSpaces)
        .WhereElementIsNotElementType()
        .Cast<Space>()
        .ToList();

    int updated = 0;

    using (Transaction trans = new Transaction(doc, "Копирование параметра из помещений"))
    {
        trans.Start();

        foreach (var space in spaces)
        {
            string number = space.Number;
            string name = space.Name;

            var matchingRoom = rooms.FirstOrDefault(r => r.Number == number && r.Name == name);
            if (matchingRoom == null)
                continue;
     
            // Получаем значение параметра "Odsunięcie ograniczenia"
            Parameter sourceParam = matchingRoom.LookupParameter("Odsunięcie ograniczenia");
            string message = String.Format("{0}, {1}, {2}",name, matchingRoom.Name, sourceParam.AsValueString());
   

            if (sourceParam == null)
                continue;
       

            string value = sourceParam.AsValueString();

            if (!string.IsNullOrEmpty(value))
            {
                Parameter targetParam = space.LookupParameter("Komentarze");
      
                if (targetParam != null)
                {
                    targetParam.Set(value);
                    updated++;
                }
            }
        }

        trans.Commit();
    }

    TaskDialog.Show("Готово", updated.ToString());
}

		
		public void AirTerminalsLeveling()
{
    Document doc = this.ActiveUIDocument.Document;

    // 1. Получаем все воздухораспределители
    FilteredElementCollector airTerminalsCollector = new FilteredElementCollector(doc);
    List<Element> airTerminals = airTerminalsCollector
        .OfCategory(BuiltInCategory.OST_DuctTerminal)
        .WhereElementIsNotElementType()
        .ToList();
        // 2. Оставляем только с одним коннектором
    List<Element> singleConnectorAirTerminals = new List<Element>();
    foreach (Element airTerminal in airTerminals)
    {
        
FamilyInstance fi = airTerminal as FamilyInstance;
if (fi == null) continue;

MEPModel mepModel = fi.MEPModel;
if (mepModel == null) continue;

        if (mepModel == null) continue;

        ConnectorSet connectors = mepModel.ConnectorManager.Connectors;
        if (connectors.Size == 1)
        {
            singleConnectorAirTerminals.Add(airTerminal);
        }
    }

    // 3. Коллекция интересующих воздухораспределителей
    List<Element> selectedAirTerminals = new List<Element>();
    

    // 4. Добавляем те, что напрямую подключены к гибкому воздуховоду
    foreach (Element airTerminal in singleConnectorAirTerminals)
    {
        
FamilyInstance fi = airTerminal as FamilyInstance;
if (fi == null) continue;

MEPModel mepModel = fi.MEPModel;
if (mepModel == null) continue;

        if (mepModel == null) continue;

        ConnectorSet connectors = mepModel.ConnectorManager.Connectors;

        foreach (Connector connector in connectors)
        {
            ConnectorSet connected = connector.AllRefs;

            foreach (Connector conn in connected)
            {
                Element connectedElement = conn.Owner;

                if (connectedElement.Category != null &&
                    connectedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexDuctCurves)
                {
                    selectedAirTerminals.Add(airTerminal);
                    goto NextTerminal;
                }
            }
        }

    NextTerminal:
        continue;
    }
    
    // 5. Удаляем уже выбранные из общего списка
    airTerminals = airTerminals
        .Where(at => !selectedAirTerminals.Contains(at))
        .ToList();

    // 6. Проверяем подключённые элементы на связь с гибким воздуховодом
    foreach (Element airTerminal in airTerminals)
    {
        FamilyInstance fi = airTerminal as FamilyInstance;
if (fi == null) continue;

MEPModel mepModel = fi.MEPModel;
if (mepModel == null) continue;
        if (mepModel == null) continue;

        ConnectorSet connectors = mepModel.ConnectorManager.Connectors;

        foreach (Connector connector in connectors)
        {
            ConnectorSet connected = connector.AllRefs;

            foreach (Connector conn in connected)
            {
                Element connectedElement = conn.Owner;
                if (connectedElement.Id == airTerminal.Id) continue;
                FamilyInstance fiConnected = connectedElement as FamilyInstance;
if (fiConnected == null) continue;

MEPModel connectedMEP = fiConnected.MEPModel;
if (mepModel == null) continue;

                if (connectedMEP == null) continue;

                ConnectorSet connectedElementConnectors = connectedMEP.ConnectorManager.Connectors;

                foreach (Connector subConnector in connectedElementConnectors)
                {
                    ConnectorSet subConnected = subConnector.AllRefs;

                    foreach (Connector subConn in subConnected)
                    {
                        Element subConnectedElement = subConn.Owner;

                        if (subConnectedElement.Category != null &&
                            subConnectedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexDuctCurves)
                        {
                            selectedAirTerminals.Add(airTerminal);
                            goto NextAirTerminal;
                        }
                    }
                }
            }
        }

    NextAirTerminal:
        continue;
    }
    

    
    List<Room> rooms = GetRoomsFromLinkedDocuments();
        
List<Element> filteredAirTerminals = new List<Element>();
string roomsName = "";
foreach (var room in rooms) {
	Parameter offsetParam = room.LookupParameter("Odsunięcie ograniczenia");
	double offsetFeet = offsetParam.AsDouble();
	double offsetMillimeters = offsetFeet * 304.8;
	roomsName += Environment.NewLine + room.Number + "-" + room.Name + "-" + offsetMillimeters.ToString();
}

UiHelper.ShowLogForm(roomsName);

foreach (Element airTerminal in selectedAirTerminals)
{
    LocationPoint locationPoint = airTerminal.Location as LocationPoint;
    if (locationPoint == null) continue;

    XYZ point = locationPoint.Point;
    Space space = doc.GetSpaceAtPoint(point);

    if (space != null)
    {
        string spaceName = space.Name;
        string spaceNumber = space.Number;

        // Ищем соответствующее помещение в архитектурной модели
        bool matchingRoomExists = rooms.Any(room =>
            room.Name == spaceName &&
            room.Number == spaceNumber);

        if (matchingRoomExists)
        {
            filteredAirTerminals.Add(airTerminal);
        }
    }
}


List<Element> excludedAirTerminals = selectedAirTerminals
    .Where(at => !filteredAirTerminals.Contains(at))
    .ToList();


// Обновляем коллекцию
selectedAirTerminals = filteredAirTerminals;


using (Transaction trans = new Transaction(doc, "Запись комментария"))
{
    trans.Start();

    foreach (Element airTerminal in excludedAirTerminals)
    {
        Parameter commentParam = airTerminal.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

        if (commentParam != null && !commentParam.IsReadOnly)
        {
            commentParam.Set("нету соответствия между space и room");
        }
    }

    trans.Commit();
}

List<Element> newFilteredAirTerminals = new List<Element>();

foreach (Element airTerminal in selectedAirTerminals)
{
    LocationPoint locationPoint = airTerminal.Location as LocationPoint;
    if (locationPoint == null) continue;

    XYZ currentPoint = locationPoint.Point;
    Space space = doc.GetSpaceAtPoint(currentPoint);

    if (space != null)
    {
        string spaceName = space.Name;
        string spaceNumber = space.Number;

        // Ищем соответствующее помещение
        Room matchingRoom = rooms.FirstOrDefault(room =>
            room.Name == spaceName &&
            room.Number == spaceNumber);

        if (matchingRoom != null)
        {
            // Получаем параметр "Odsunięcie ograniczenia" (Limit Offset)
            

   Parameter offsetParam = matchingRoom.LookupParameter("Odsunięcie ograniczenia");

    if (offsetParam != null && offsetParam.StorageType == StorageType.Double)
    {
        double offsetFeet = offsetParam.AsDouble(); // значение в футах
       if (offsetFeet != 0) 
       {
       	
        Parameter levelTerm = airTerminal.LookupParameter("Odsunięcie");
       	
		double levelTermDouble = levelTerm.AsDouble();
		if (levelTerm != null && !levelTerm.IsReadOnly && levelTermDouble != offsetFeet)
        {
        using (Transaction trans = new Transaction(doc, "Установка высоты воздухораспределителя"))
        {
            trans.Start();

            levelTerm.Set(offsetFeet);

            trans.Commit();
        }
		}
    }
    }

        }
    }
}




    // 7. Выводим результат
    string messageEnd = string.Format("Найдено воздухораспределителей, подключённых к гибким воздуховодам: {0}", selectedAirTerminals.Count);
    string info = GetClassInfoString(selectedAirTerminals[1]);
    UiHelper.ShowLogForm(info);
    TaskDialog.Show("Результат", messageEnd);
    
    
}
		
		
		
		

public List<Room> GetRoomsFromLinkedDocuments()
{
    Document doc = this.ActiveUIDocument.Document;
    List<Room> allRooms = new List<Room>();

    // Получаем все экземпляры связанных моделей
    FilteredElementCollector linkCollector = new FilteredElementCollector(doc)
        .OfClass(typeof(RevitLinkInstance));

    foreach (RevitLinkInstance linkInstance in linkCollector)
    {
        Document linkedDoc = linkInstance.GetLinkDocument();
        if (linkedDoc == null) continue;

        // Получаем все помещения (Room) из связанного документа
        FilteredElementCollector roomCollector = new FilteredElementCollector(linkedDoc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

        foreach (Element roomElement in roomCollector)
        {
            Room room = roomElement as Room;
            if (room != null)
            {
                allRooms.Add(room);
            }
        }
    }

    return allRooms;
}

public void FindVentilationElementsWithSystemCZ4()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    string targetSystemName = Interaction.InputBox( "Введите название системы вентиляции:", "Поиск системы", "CZ4");
    
    if (string.IsNullOrWhiteSpace(targetSystemName)) { UiHelper.ShowLogForm("Не введено название системы."); return; }

    var collector = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_DuctSystem)
        .WhereElementIsNotElementType();

    List<ElementId> ids = new List<ElementId>();

    foreach (var elem in collector)
    {
        Parameter p = elem.LookupParameter("Nazwa systemu");
        if (p != null && p.HasValue)
        {
            string value = p.AsString();
            if (value == targetSystemName)
            {
                ids.Add(elem.Id);
            }
        }
    }

    // Формируем строку для вывода
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("Znaleziono " + ids.Count + " elementów systemu" + targetSystemName + ":");
    foreach (var id in ids)
    {
        sb.AppendLine(id.IntegerValue.ToString());
    }

    // Выводим в твоё окно логов
    UiHelper.ShowLogForm(sb.ToString());
}

public void ReportCeilingsByRooms()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    // Сбор помещений
    var rooms = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Rooms)
        .WhereElementIsNotElementType()
        .Cast<Room>()
        .ToList();

    // Сбор потолков
    var ceilings = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Ceilings)
        .WhereElementIsNotElementType()
        .ToList();

    // Словарь: помещение → список потолков
    Dictionary<ElementId, List<Element>> roomCeilings = new Dictionary<ElementId, List<Element>>();

    foreach (var room in rooms)
        roomCeilings[room.Id] = new List<Element>();

    // Список потолков, не попавших ни в одно помещение
    List<Element> orphanCeilings = new List<Element>();

    // Определяем помещение для каждого потолка
    foreach (Element ceiling in ceilings)
    {
        BoundingBoxXYZ bb = ceiling.get_BoundingBox(null);
        if (bb == null)
        {
            orphanCeilings.Add(ceiling);
            continue;
        }

        XYZ center = (bb.Min + bb.Max) * 0.5;
        XYZ testPoint = center - new XYZ(0, 0, 0.1); // опускаем точку вниз
        Room room = doc.GetRoomAtPoint(testPoint);
        
//        Room room = GetRoomByCeiling(doc, ceiling);

        if (room != null)
        {
            roomCeilings[room.Id].Add(ceiling);
        }
        else
        {
            orphanCeilings.Add(ceiling);
        }
    }

    // Формируем итоговый текст
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("ОТЧЁТ ПО ПОТОЛКАМ:");
    sb.AppendLine();

    // Для каждого помещения
    foreach (var kvp in roomCeilings)
    {
        Room room = doc.GetElement(kvp.Key) as Room;
        List<Element> rc = kvp.Value;

        string roomHeader = room.Number + " " + room.Name + " – ";

        if (rc.Count == 0)
        {
            sb.AppendLine(roomHeader + "потолка нету");
            continue;
        }

        // собираем высоты потолков
        List<string> heights = new List<string>();

        foreach (var c in rc)
        {
            Parameter h = c.LookupParameter("Odsunięcie pionowe od poziomu odniesienia");
            double height = h != null ? h.AsDouble() : 0;

            // Перевод в метры
            double meters = UnitUtils.ConvertFromInternalUnits(height, DisplayUnitType.DUT_METERS);

            heights.Add(meters.ToString("0.000"));
        }

        sb.AppendLine(roomHeader + string.Join(", ", heights));
    }

    sb.AppendLine();
    sb.AppendLine("ПОТОЛКИ БЕЗ ПОМЕЩЕНИЙ:");
    sb.AppendLine();

    foreach (var c in orphanCeilings)
    {
        Parameter h = c.LookupParameter("Odsunięcie pionowe od poziomu odniesienia");
        double height = h != null ? h.AsDouble() : 0;
        double meters = UnitUtils.ConvertFromInternalUnits(height, DisplayUnitType.DUT_METERS);

        Parameter areaP = c.LookupParameter("Area");
        double area = areaP != null ? areaP.AsDouble() : 0;
        double areaM2 = UnitUtils.ConvertFromInternalUnits(area, DisplayUnitType.DUT_SQUARE_METERS);
        string id = c.Id.ToString();

        sb.AppendLine(meters.ToString("0.000") + " – " + areaM2.ToString("0.0") + " м2" + " - " + id);
    }

    // Вывод результата
    UiHelper.ShowLogForm(sb.ToString());
}

public void ChangeHeightRoomsByCeilings()
{
    UIDocument uidoc = this.ActiveUIDocument;
    Document doc = uidoc.Document;

    // Сбор помещений
    var rooms = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Rooms)
        .WhereElementIsNotElementType()
        .Cast<Room>()
        .ToList();

    // Сбор потолков
    var ceilings = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Ceilings)
        .WhereElementIsNotElementType()
    	.Cast<Ceiling>() 
        .ToList();
    
    // Сбор перекрытий
    var floors = new FilteredElementCollector(doc)
    	.OfCategory(BuiltInCategory.OST_Floors)
    	.WhereElementIsNotElementType()
    	.Cast<Floor>()
    	.ToList();  



    
        // Заполнение списка редактора помещени	
        List<RoomInfo> roomInfosAll = new List<RoomInfo>();
        List<RoomInfo> roomInfos = new List<RoomInfo>();        
    foreach (var room in rooms) {
    	
        	if (room != null) {
        		
        		RoomInfo roomInf = new RoomInfo(doc, room);
        		roomInfosAll.Add(roomInf);
        	}
    }
        
    // Сбор этажей
	var levels = roomInfosAll
	    .Select(r => r.LevelName)
	    .Distinct()
	    .OrderBy(n => n)
	    .ToList();
	
	// Собираем форму выбора этажей
		var levelElements = new Dictionary<ElementId, string>();
		
		foreach (var roomInf in roomInfosAll)
		{
		    if (!levelElements.ContainsKey(roomInf.LevelId))
		    {
		        levelElements.Add(roomInf.LevelId, roomInf.LevelName);
		    }
		}
		var formForLevels = new ElementSelectorForm(levelElements);

		    if (formForLevels.ShowDialog() != System.Windows.Forms.DialogResult.OK)
		        return;
		
		    // Получаем выбранные элементы
		    var selectedLevels = formForLevels.SelectedElements;
		    
		foreach (var roomInf in roomInfosAll) {
        	
	        	
	        	if (selectedLevels.ContainsKey(roomInf.LevelId))
					{
					    roomInfos.Add(roomInf);
					}

	        }
	
        // Установка низа огранищивающего перекрытия
        foreach (var roomInf in roomInfos) {
        	
        	roomInf.PutSlabBottomElevation(floors);
        }
        
        //Проверяем offcet
//		StringBuilder sb2 = new StringBuilder();
//		
//		sb2.AppendLine("ОТЧЁТ ПО Высоте ограничений:");
//		sb2.AppendLine();
		
		foreach (var roomInf in roomInfos) {
        	
        	string roomHeader = roomInf.RoomElement.Number + " " + roomInf.RoomElement.Name + " – ";
//        	sb2.AppendLine(roomHeader + roomInf.Ceilings.Count.ToString());
        }
		

		

        
        // Установка высоты поещениния в зависимости от перекрытий
        
        using (Transaction t = new Transaction(doc, "Update Rooms"))
			{
			    t.Start();
			
			    foreach (var ri in roomInfos)
			    {
			    	Room room = ri.RoomElement;
						//string roomHeader = ri.RoomElement.Number + " " + ri.RoomElement.Name + " – ";
						// Уровень, к которому привязана верхняя граница
						Level upperLevel = doc.GetElement(
						    room.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL).AsElementId()
						) as Level;
						
						double upperLevelElevation = upperLevel.Elevation;
						
						// Новый offset
						double newOffset = ri.SlabBottomElevation - upperLevelElevation;
						
						//double height = upperLevelElevation * 0.3048;
		    			//sb2.AppendLine(roomHeader + height.ToString());
						
						// Устанавливаем offset
						Parameter upperOffset = room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
						if (!upperOffset.IsReadOnly)
						{
						    upperOffset.Set(newOffset);
						}
			    }
			
			    t.Commit();
			}        

        // Поиск подвесных потолков для каждого помещения
        
            foreach (var ceiling in ceilings)
		    {
		        BoundingBoxXYZ bb = ceiling.get_BoundingBox(null);
		        if (bb == null)
		        {
		            continue;
		        }
		
		        XYZ center = (bb.Min + bb.Max) * 0.5;
		        
		        XYZ testPoint = center - new XYZ(0, 0, 0.1); // опускаем точку вниз
        		Room room = doc.GetRoomAtPoint(testPoint);
		        if (room != null)
		        {
		        	var roomInf = roomInfos.Find(p => p.Id == room.Id);
		        	if(roomInf != null)
		        	{
		        		roomInf.AddCelling(ceiling);
		        	}
		        }

            }
            
            //Логируем высоту потолка
//		    StringBuilder sb = new StringBuilder();
//		
//		    sb.AppendLine("ОТЧЁТ ПО ПОТОЛКАМ:");
//		    sb.AppendLine();
		    
//		    foreach (var ri in roomInfos) {
//		    	string roomHeader = ri.RoomElement.Number + " " + ri.RoomElement.Name + " – ";
//		    	double height = ri.SlabBottomElevation * 0.3048;
//		    	sb.AppendLine(roomHeader + height.ToString());
//		    	
//		    }
		    
		    
//		    ShowLogForm(sb2.ToString());
            
            // Меняем высоту помещения в RoomInfo в зависимости от высоты потолков
            foreach (var ri in roomInfos) {
            	ri.PutSlabBottomElevationByCeiling();
            }
            
            var roomElements = new Dictionary<ElementId, string>();
		
				foreach (var roomInf in roomInfos) {
		        	
					double height = (roomInf.SlabBottomElevation - roomInf.Level.Elevation) * 0.3048;
					
					string roomHeader = roomInf.RoomElement.Number + " " + roomInf.RoomElement.Name + " – " + roomInf.LevelName + " - " + height.ToString();
		        	roomElements.Add(roomInf.Id, roomHeader);
		        }
		    
		    var form = new ElementSelectorForm(roomElements);

		    if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
		        return;
		
		    // Получаем выбранные элементы
		    var selected = form.SelectedElements;
		
		    TaskDialog.Show("Info", selected.Count.ToString());
		    
		    foreach (var element in selected) {
		    	
		    	RoomInfo ri = roomInfos.Find(p => p.Id == element.Key);
		    	if(ri != null){
		    		ri.WillBeChanged = true;
		    	}
		    }
            

            
                    using (Transaction t = new Transaction(doc, "Update Rooms"))
			{
			    t.Start();
			    
			    foreach (var ri in roomInfos)
			    {
			    	Room room = ri.RoomElement;
			    	Parameter upperOffset = room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
						if (!upperOffset.IsReadOnly)
						{						    
					    	if(ri.WillBeChanged){			    		
		
								// Уровень, к которому привязана верхняя граница
								Level upperLevel = doc.GetElement(
								    room.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL).AsElementId()
								) as Level;
								
								double upperLevelElevation = upperLevel.Elevation;
								
								// Новый offset
								double newOffset = ri.SlabBottomElevation - upperLevelElevation;
								upperOffset.Set(newOffset);
								
					    	}
					    	else {
								upperOffset.Set(ri.StartHeight);
					    		
					    	}				

					}
			    }			

			
			    t.Commit();
			}
            
            
        
}
  

public string GetClassInfoString(object obj)
{
    if (obj == null)
        return "Объект не задан.";

    Type type = obj.GetType();
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("Класс: " + type.FullName);

    // Свойства
    sb.AppendLine("\nСвойства:");
    foreach (PropertyInfo prop in type.GetProperties())
    {
        sb.AppendLine("- " + prop.Name + " : " + prop.PropertyType.Name);
    }

    // Методы
    sb.AppendLine("\nМетоды:");
    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        sb.AppendLine("- " + method.Name + "()");
    }

    // Конструкторы
    sb.AppendLine("\nКонструкторы:");
    foreach (ConstructorInfo ctor in type.GetConstructors())
    {
        sb.Append("- " + type.Name + "(");
        ParameterInfo[] parameters = ctor.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            sb.Append(parameters[i].ParameterType.Name + " " + parameters[i].Name);
            if (i < parameters.Length - 1) sb.Append(", ");
        }
        sb.AppendLine(")");
    }

    return sb.ToString();
}

public void ElemetsNaming()
{
		    Document doc = this.ActiveUIDocument.Document;

		IList<Element> elements = new List<Element>();
		
		// Категории
		BuiltInCategory[] categories = new BuiltInCategory[]
		{
		    BuiltInCategory.OST_DuctCurves,           // Воздуховоды
		    BuiltInCategory.OST_FlexDuctCurves,       // Гибкие воздуховоды
		    BuiltInCategory.OST_DuctTerminal,         // Воздухораспределители
		    BuiltInCategory.OST_DuctAccessory,        // Аксессуары
		    BuiltInCategory.OST_DuctFitting           // Фитинги
		};
		
		foreach (var cat in categories)
		{
		    
		var collector = new FilteredElementCollector(doc)
		        .WherePasses(new ElementCategoryFilter(cat))
		        .WhereElementIsNotElementType(); // чтобы не брать типы

		    foreach (var e in collector)
		        elements.Add(e);
		}
		string allSystemNames = string.Empty;
		
		allSystemNames += "Общее количество элементов: " + elements.Count + Environment.NewLine + Environment.NewLine;
		// Коллекция для хранения объектов SystemElements
		List<SystemElements> systemsCollection = new List<SystemElements>();
		
		// Проходим по всем элементам
		foreach (Element elem in elements)
		{
		    // Читаем параметр "Nazwa systemu"
		    Parameter sysParam = elem.LookupParameter("Nazwa systemu");
		    if (sysParam == null || string.IsNullOrEmpty(sysParam.AsString()))
		        continue; // Если параметра нет или пустой, пропускаем
		
		    string systemName = sysParam.AsString();
		
		    // Ищем существующий объект SystemElements с таким именем
		    SystemElements existingSystem = systemsCollection
		        .FirstOrDefault(s => s.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
		
		    if (existingSystem != null)
		    {
		        // Добавляем элемент в найденную систему
		        existingSystem.AddElement(elem);
		    }
		    else
		    {
		        // Создаём новый объект и добавляем элемент
		        SystemElements newSystem = new SystemElements(systemName);
		        newSystem.AddElement(elem);
		        systemsCollection.Add(newSystem);
		    }
		}
		
		
		
		

		int totalCount = 0;
		foreach (SystemElements sys in systemsCollection)
		{
		    allSystemNames += sys.SystemName + " - " + sys.Elements.Count + Environment.NewLine;
		    totalCount += sys.Elements.Count; // суммируем количество элементов
		}
		
		// Добавляем итоговую сумму
		allSystemNames += Environment.NewLine + "Сумма всех элементов: " + totalCount;


		UiHelper.ShowLogForm(allSystemNames);		
		
		
		using (Transaction t = new Transaction(doc, "Renumber Elements"))
		{
		    t.Start();
		
		    foreach (SystemElements sys in systemsCollection)
		    {
		        sys.RenumberElements("Komentarze");
		    }
		
		    t.Commit();
		}



		
		
		

}


public Room GetRoomByCeiling(Document doc, Element ceiling)
{
    SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
    SpatialElementGeometryCalculator calc = new SpatialElementGeometryCalculator(doc, opt);

    // Геометрия потолка
    Options gopt = new Options();
    gopt.ComputeReferences = true;
    gopt.DetailLevel = ViewDetailLevel.Fine;

    GeometryElement ceilingGeom = ceiling.get_Geometry(gopt);
    if (ceilingGeom == null)
        return null;

    // Собираем помещения
    FilteredElementCollector rooms = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Rooms)
        .WhereElementIsNotElementType();

    foreach (Room room in rooms)
    {
        if (room == null)
            continue;

        SpatialElementGeometryResults results = null;

        try
        {
            results = calc.CalculateSpatialElementGeometry(room);
        }
        catch
        {
            continue; // помещение может быть некорректным
        }

        if (results == null)
            continue;

        Solid roomSolid = results.GetGeometry();
        if (roomSolid == null || roomSolid.Volume == 0)
            continue;

        // Проверяем пересечение с потолком
        foreach (GeometryObject go in ceilingGeom)
        {
            Solid ceilingSolid = go as Solid;
            if (ceilingSolid == null || ceilingSolid.Volume == 0)
                continue;

            Solid intersection = null;

            try
            {
                intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    roomSolid,
                    ceilingSolid,
                    BooleanOperationsType.Intersect
                );
            }
            catch
            {
                continue;
            }

            if (intersection != null && intersection.Volume > 1e-6)
            {
                return room; // потолок принадлежит этому помещению
            }
        }
    }

    return null; // потолок не попал ни в одно помещение
}


	}
	


	


    

}