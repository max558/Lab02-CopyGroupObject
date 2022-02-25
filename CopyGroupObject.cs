using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                GroupPickFilter groupPickFilter = new GroupPickFilter();
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, groupPickFilter, "Выберитегруппу объектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;

                XYZ groupCenter = GetElementCenter(group);
                if (group == null)
                {
                    return Result.Cancelled;
                }
                //Нахождения комнаты, в которой выбрана группа и смещение центров комнаты и группы
                Room room = GetRoomByPoint(doc,groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;

                XYZ point = uiDoc.Selection.PickPoint("Выберите точку вставки");

                //Определение точки вставки скопированной группы с теми же смещениями от центра комнаты, что и выбранная группа
                Room newRoom = GetRoomByPoint(doc, point);
                XYZ newRoomCenter = GetElementCenter(newRoom);
                XYZ poinPush = offset + newRoomCenter;

                using (Transaction ts = new Transaction(doc, "Копирование группы объектов"))
                {
                    ts.Start();
                    doc.Create.PlaceGroup(poinPush, group.GroupType);
                    ts.Commit();
                }
                //Transaction ts = new Transaction(doc);
                //ts.Start("Копирование группы объектов");
                //doc.Create.PlaceGroup(point, group.GroupType);
                //ts.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception es)
            {
                message = es.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }
        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element item in collector)
            {
                Room room = item as Room;
                if (room == null)
                {
                    continue;
                }
                if (room.IsPointInRoom(point))
                {
                    return room;
                }
            }
            return null;
        }
    }

    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
