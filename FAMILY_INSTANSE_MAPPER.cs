using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Reference = Autodesk.Revit.DB.Reference;
using System.Collections.Generic;
using System.Windows;
using System;


namespace FAMILY_INSTANSE_MAPPER
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class FAMILY_INSTANSE_MAPPER : IExternalCommand
    {
        private Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            _doc = uiDoc.Document;
            
            Reference firstRef  = uiDoc.Selection.PickObject(ObjectType.Element,
                    "Выберите арматурный каркас колонны");
            if (firstRef == null)
            {
                //TODO сделать окно с циклом. выход из цикла - нажать "Отмена" или выбор арматурного каркаса
                //TODO сделать проверку на то, что это арматурный каркас
                TaskDialog.Show("Ошибка", $"Не был выбран арматурный каркас колонны");
                return Result.Failed;
            }
            
            Reference secondRef = uiDoc.Selection.PickObject(ObjectType.Element,
                    "Блок соединителей арматуры");
            if (secondRef == null)
            {//TODO сделать окно с циклом. выход из цикла - нажать "Отмена" или выбор блок соединителей арматуры
                //TODO сделать проверку на то, что это блок соединителей арматуры
                TaskDialog.Show("Ошибка", $"Не был выбран блок соединителей арматуры");
                return Result.Failed;
            }
            

            using (Transaction trans = new Transaction(_doc, "Сопоставление соединителей с арматурой"))
            {
                trans.Start();

                Element firstElement = _doc.GetElement(firstRef);
                Element secondElement = _doc.GetElement(secondRef);

                List<string> parameters = new List<string>();
                parameters.AddRange(new[] 
                {
                    "Количество_Длина",
                    "Количество_Ширина",
                    "Колонна_Длина",
                    "Колонна_Ширина",
                    "Колонна_Высота",
                    "Смещение_ВБ_Снизу",
                    "Разбежка_Снизу",
                    "Смещение_ВБ_Сверху",
                    "Разбежка_Сверху",
                    "Расстояние от грани колонны до центра стержня",
                    "Диаметр_ВБ"
                });

                foreach (string parameter in parameters)
                {
                    try
                    {
                        string ParamName = parameter;
                        string data = GetParameterValue(firstElement, ParamName);
                        SetValueToElementParameter(secondElement, ParamName, data);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"НЕ Успешно\n{ex}");
                        return Result.Failed;
                    }
                }

                trans.Commit();
            }
            return Result.Succeeded;
        }

        private string GetParameterValue
            (Element element, string ParameterName)

        {
            Parameter parameter = GetElementParameterByName
                (element, ParameterName);
             return parameter.AsValueString();
        }
        private void SetValueToElementParameter
            (Element element, string ParameterName, string Value)
        {
            Parameter parameter = GetElementParameterByName(element, ParameterName);
            string dataType = parameter.Definition.ParameterType.ToString();
            switch (dataType)
            {
                case "String":
                    {
                        parameter.Set(Value);
                        break;
                    }
                case "Integer":
                    {
                        parameter.Set(Int32.Parse(Value));
                        break;
                    }
                case "Length":
                    {
                        parameter.Set(Int32.Parse(Value) /304.8);
                        break;
                    }
                case "BarDiameter":
                    {
                        char[] chars = new char[2];
                        chars[0] = ' ';
                        chars[1] = 'm';

                        Value = Value.Trim(chars);
                        parameter.Set(Int32.Parse(Value)/304.8);
                        break;
                    }
            }
        }

        private Parameter GetElementParameterByName
            (Element element, string ParameterName)
        {
            Parameter parameterInstance = element.LookupParameter(ParameterName);
            if (parameterInstance != null)
            { return parameterInstance; }

            ElementId typeId = element.GetTypeId();
            if (typeId == ElementId.InvalidElementId)
                return null;

            Element typeElement = _doc.GetElement(typeId);
            if (typeElement == null)
                return null;

            Parameter parameterType = typeElement.LookupParameter(ParameterName);
            return parameterType;

        }
    }
}
