using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;

namespace TestPik
{

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class CMDColorRooms : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData,
    ref string message, Autodesk.Revit.DB.ElementSet elements)
        {            
            try
            {
                if (null == commandData)
                {
                    throw new ArgumentNullException("commandData");
                }
                // получаем документ
                Document doc = commandData.Application.ActiveUIDocument.Document;
                // раскрашиваем. Транзакция там
                GetRooms.GetRoomsPIK(doc);
                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return Autodesk.Revit.UI.Result.Failed;
            }
        }
    }
}