using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupObject
{
    [Transaction(TransactionMode.Manual)]
    public class CopyGroupObject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Выберитегруппу объектов");
            Element element = doc.GetElement(reference);
            Group group = element as Group;

            if (group == null)
            {
                return Result.Cancelled;
            }

            XYZ point = uiDoc.Selection.PickPoint("Выберите точку вставки");

            
            using (Transaction ts = new Transaction(doc, "Копирование группы объектов"))
            {
                ts.Start();
                doc.Create.PlaceGroup(point, group.GroupType);
                ts.Commit();
            }
            //Transaction ts = new Transaction(doc);
            //ts.Start("Копирование группы объектов");
            //doc.Create.PlaceGroup(point, group.GroupType);
            //ts.Commit();
            
            return Result.Succeeded;
        }
    }
}
